using System;
namespace MFractor.iOS.Images
{
    public class AssetCatalogueInfo
    {
        [Newtonsoft.Json.JsonProperty("author")]
        public string Author { get; set; }

        [Newtonsoft.Json.JsonProperty("version")]
        public int Version { get; set; }
    }
}
