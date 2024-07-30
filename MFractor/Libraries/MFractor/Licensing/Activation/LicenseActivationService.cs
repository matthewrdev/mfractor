using System;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MFractor.Licensing.Activation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILicenseActivationService))]
    class LicenseActivationService : ILicenseActivationService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<ISharedHttpClient> sharedHttpClient;
        public ISharedHttpClient SharedHttpClient => sharedHttpClient.Value;

        readonly Lazy<ILicenseRequestFactory> licenseRequestFactory;
        public ILicenseRequestFactory LicenseRequestFactory => licenseRequestFactory.Value;

        readonly Lazy<IApiConfig> apiConfig;
        public IApiConfig ApiConfig => apiConfig.Value;

        HttpClient HttpClient => SharedHttpClient.HttpClient;

        [ImportingConstructor]
        public LicenseActivationService(Lazy<ISharedHttpClient> sharedHttpClient,
                                        Lazy<ILicenseRequestFactory> licenseRequestFactory,
                                        Lazy<IApiConfig> apiConfig)
        {
            this.sharedHttpClient = sharedHttpClient;
            this.licenseRequestFactory = licenseRequestFactory;
            this.apiConfig = apiConfig;
        }

        public async Task<ILicenseRequestResult> ActivateSerialKey(string emailAddress, string serialKey)
        {
            const string api = "/Activation";

            var licenseRequest = LicenseRequestFactory.CreateSerialKeyRequest(emailAddress, serialKey);

            var json = JsonConvert.SerializeObject(licenseRequest);

            var request = new HttpRequestMessage(HttpMethod.Post, ApiConfig.Endpoint + api);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("APIKEY", ApiConfig.ApiKey);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            return await RequestLicense(request);
        }

        public async Task<ILicenseRequestResult> ActivateTrialLicense(string emailAddress, string licenseeName)
        {
            const string api = "/Trial";

            var licenseRequest = LicenseRequestFactory.CreateTrialLicenseRequest(emailAddress, licenseeName);

            var json = JsonConvert.SerializeObject(licenseRequest);

            var request = new HttpRequestMessage(HttpMethod.Post, ApiConfig.Endpoint + api);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("APIKEY", ApiConfig.ApiKey);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            return await RequestLicense(request);
        }

        async Task<ILicenseRequestResult> RequestLicense(HttpRequestMessage request)
        {
            var response = await HttpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK
                || response.StatusCode == System.Net.HttpStatusCode.Created
                || response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var body = await response.Content.ReadAsStringAsync();
                log?.Info($"License request success: " + response.StatusCode);

                var jsonObject = JObject.Parse(body);

                var licenseContent = jsonObject["licenseDataBase64"].Value<string>();

                return new LicenseRequestResult(true, licenseContent, string.Empty, string.Empty);
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

                    return new LicenseRequestResult(false, string.Empty, title, detail);
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