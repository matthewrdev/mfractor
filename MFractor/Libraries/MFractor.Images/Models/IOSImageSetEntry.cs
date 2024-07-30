using System;
using Newtonsoft.Json;

namespace MFractor.Images.Models
{
    public class IOSImageSetEntry
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("scale")]
        public string Scale { get; set; }

        [JsonProperty("idiom")]
        public string Idiom { get; set; } = "universal";

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("subtype")]
        public string SubType { get; set; }
    }
}
