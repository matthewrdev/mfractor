using System;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MFractor.Licensing.Recovery
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILicenseRecoveryService))]
    class LicenseRecoveryService : ILicenseRecoveryService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<ISharedHttpClient> sharedHttpClient;
        public ISharedHttpClient SharedHttpClient => sharedHttpClient.Value;

        readonly Lazy<IApiConfig> apiConfig;
        public IApiConfig ApiConfig => apiConfig.Value;

        HttpClient HttpClient => SharedHttpClient.HttpClient;

        [ImportingConstructor]
        public LicenseRecoveryService(Lazy<ISharedHttpClient> sharedHttpClient,
                                        Lazy<IApiConfig> apiConfig)
        {
            this.sharedHttpClient = sharedHttpClient;
            this.apiConfig = apiConfig;
        }

        public async Task<ILicenseRecoveryResult> RecoverLicense(string emailAddress)
        {
            const string api = "/RecoverLicense";

            var recoveryRequest = new LicenseRecoveryRequest()
            {
                Email = emailAddress,
            };

            var json = JsonConvert.SerializeObject(recoveryRequest);

            var request = new HttpRequestMessage(HttpMethod.Post, ApiConfig.Endpoint + api);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("APIKEY", ApiConfig.ApiKey);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            return await RecoverLicense(request);
        }

        async Task<ILicenseRecoveryResult> RecoverLicense(HttpRequestMessage request)
        {
            var response = await HttpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK
                || response.StatusCode == System.Net.HttpStatusCode.Created
                || response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                return new LicenseRecoveryResult("A request to recover your MFractor license for {email} has been raised.\n\nIf this email address is associated with a valid license, you will recieve your license soon.", true);
            }
            else
            {
                try
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {

                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {

                    }

                    var body = await response.Content.ReadAsStringAsync();

                    var jsonObject = JObject.Parse(body);

                    var title = string.Empty;
                    var detail = string.Empty;


                    if (jsonObject.TryGetValue("errorMessage", out var errorMessageValue))
                    {
                        title = errorMessageValue.Value<string>();
                        detail = "This machine or email address has already been issued a 30-day trial of MFractor Professional.\nYou are now using MFractor Lite.";
                    }
                    else
                    {
                        if (jsonObject.TryGetValue("title", out var titleValue))
                        {
                            title = titleValue.Value<string>();
                        }

                        if (jsonObject.TryGetValue("detail", out var detailValue))
                        {
                            detail = detailValue.Value<string>();
                        }
                    }

                    log?.Warning("License request error: " + response.StatusCode);
                    log?.Warning($"{title} - {detail}");

                    return new LicenseRecoveryResult(title, false);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                    throw ex;
                }
            }
        }
    }
}