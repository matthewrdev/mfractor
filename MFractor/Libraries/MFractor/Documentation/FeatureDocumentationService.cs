using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MFractor.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MFractor.Documentation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFeatureDocumentationService))]
    class FeatureDocumentationService : IFeatureDocumentationService, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();
        [ImportingConstructor]        public FeatureDocumentationService(Lazy<IUserOptions> userOptions,
                                           Lazy<IApplicationPaths> applicationPaths,
                                           Lazy<ISharedHttpClient> sharedHttpClient,
                                           Lazy<IApiConfig> apiConfig)
        {
            this.userOptions = userOptions;
            this.applicationPaths = applicationPaths;
            this.sharedHttpClient = sharedHttpClient;
            this.apiConfig = apiConfig;
        }

        readonly Lazy<IUserOptions> userOptions;
        IUserOptions UserOptions => userOptions.Value;

        readonly Lazy<IApplicationPaths> applicationPaths;
        IApplicationPaths ApplicationPaths => applicationPaths.Value;

        readonly Lazy<ISharedHttpClient> sharedHttpClient;
        public ISharedHttpClient SharedHttpClient => sharedHttpClient.Value;

        readonly Lazy<IApiConfig> apiConfig;
        public IApiConfig ApiConfig => apiConfig.Value;

        HttpClient HttpClient => SharedHttpClient.HttpClient;

        const string documentationEndpoint = "/Docs";
        const string manifestFilename = "manifest.json";

        string DocumentationUrl => ApiConfig.Endpoint + documentationEndpoint;

        const string lastSyncDateKey = "com.mfractor.docs.last_sync_date_utc";

        public string WorkingDirectory => Path.Combine(ApplicationPaths.ApplicationDataPath, "docs");

        public string ManifestPath => Path.Combine(WorkingDirectory, manifestFilename);

        public bool RequiresSync
        {
            get
            {
                var lastSync = UserOptions.Get(lastSyncDateKey, string.Empty);

                if (!DateTime.TryParse(lastSync, out var lastSyncDate))
                {
                    return true;
                }

                return (DateTime.UtcNow - lastSyncDate) > TimeSpan.FromDays(7);
            }
        }

        readonly List<IFeatureDocumentation> features = new List<IFeatureDocumentation>();
        public IReadOnlyList<IFeatureDocumentation> Features => features;

        public IReadOnlyDictionary<string, IFeatureDocumentation> DiagnosticFeatures { get; private set; } = new Dictionary<string, IFeatureDocumentation>();

        public void Shutdown()
        {
        }

        public void Startup()
        {
            Task.Run(async () =>
            {
                if (!Directory.Exists(WorkingDirectory))
                {
                    Directory.CreateDirectory(WorkingDirectory);
                }

                await SynchroniseDocumentation(false).ConfigureAwait(false);
            });
        }

        public async Task<bool> SynchroniseDocumentation(bool force)
        {
            if (!RequiresSync && !force)
            {
                BuildFeatures();
                return true;
            }

            log.Info("Starting synchronisation of feature documentation manifest from " + DocumentationUrl);

            if (File.Exists(ManifestPath))
            {
                File.Delete(ManifestPath);
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, DocumentationUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("APIKEY", ApiConfig.ApiKey);

                var content = await GetFeatureDocumentationManifestContent(request);

                if (!string.IsNullOrEmpty(content))
                {
                    File.WriteAllText(ManifestPath, content);
                }
            }
            catch
            {
                return false;
            }

            log.Info("Successfully download feature documentation manifest from " + DocumentationUrl + " into " + ManifestPath);


            BuildFeatures();

            UserOptions.Set(lastSyncDateKey, DateTime.UtcNow.ToString("O"));

            return true;
        }

        void BuildFeatures()
        {
            var features = GetFeatureDocumentationManifest();

            if (features == null)
            {
                return;
            }

            this.features.Clear();
            this.features.AddRange(features);

            var groupedFeatures = new Dictionary<string, IFeatureDocumentation>();
            foreach (var feature in features)
            {
                if (string.IsNullOrEmpty(feature.DiagnosticId))
                {
                    continue;
                }

                groupedFeatures[feature.DiagnosticId] = feature;
            }

            DiagnosticFeatures = groupedFeatures;
        }

        async Task<string> GetFeatureDocumentationManifestContent(HttpRequestMessage request)
        {
            var response = await HttpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK
                || response.StatusCode == System.Net.HttpStatusCode.Created
                || response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var body = await response.Content.ReadAsStringAsync();

                return body;
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

                    log?.Warning("Feature manifest request error: " + response.StatusCode);
                    log?.Warning($"{title} - {detail}");

                    return string.Empty;
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                    throw ex;
                }
            }
        }

        IReadOnlyList<IFeatureDocumentation> GetFeatureDocumentationManifest()
        {
            var features = new List<IFeatureDocumentation>();

            try
            {
                var contents = JsonConvert.DeserializeObject<List<FeatureDocumentation>>(File.ReadAllText(ManifestPath));

                features.AddRange(contents);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);

                return new List<IFeatureDocumentation>();
            }

            return features;
        }

        public IFeatureDocumentation GetFeatureDocumentationForDiagnostic(string diagnosticId)
        {
            if (string.IsNullOrEmpty(diagnosticId))
            {
                return default;
            }

            DiagnosticFeatures.TryGetValue(diagnosticId, out var documentation);

            return documentation;
        }
    }
}