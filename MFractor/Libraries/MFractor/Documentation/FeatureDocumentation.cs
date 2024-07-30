using System;
using Newtonsoft.Json;

namespace MFractor.Documentation
{
    public class FeatureDocumentation : IFeatureDocumentation
    {
        [JsonProperty("featureId")]
        public int FeatureId { get; set; }

        [JsonProperty("featureName")]
        public string FeatureName { get; set; }

        [JsonProperty("diagnosticId")]
        public string DiagnosticId { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public bool HasUrl => !string.IsNullOrEmpty(Url);
    }
}
