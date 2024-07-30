using Newtonsoft.Json;


namespace MFractor.Licensing.Activation
{
    class LicenseRequest
    {
        [JsonProperty("serialKey")]
        public string SerialKey { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("deviceIdentifier")]
        public string DeviceIdentifiter { get; set; }

        [JsonProperty("machineName")]
        public string MachineName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("platformVersion")]
        public string PlatformVersion { get; set; }

        [JsonProperty("visualStudioVersion")]
        public string IdeVersion { get; set; }

        [JsonProperty("mFractorVersion")]
        public string ProductVersion { get; set; }
    }
}
