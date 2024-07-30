using System;
using Newtonsoft.Json;

namespace MFractor.Images.Models
{
    public class IOSImageSetInfo
    {
        [JsonProperty("version")]
        public int Version { get; set; } = 1;

        [JsonProperty("author")]
        public string Author { get; set; } = "xcode";
    }
}
