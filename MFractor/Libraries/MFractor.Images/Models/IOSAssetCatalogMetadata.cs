using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MFractor.Images.Models
{
    public class IOSAssetCatalogMetadata
    {
        [JsonProperty("images")]
        public List<IOSImageSetEntry> Images { get; } = new List<IOSImageSetEntry>();

        [JsonProperty("info")]
        public IOSImageSetInfo Info { get; } = new IOSImageSetInfo();

        JsonSerializerSettings IgnoreNullJsonSettings => new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, };

        /// <summary>
        /// Serialize this instance into a JSON string.
        /// </summary>
        /// <returns>The JSON representation of this object.</returns>
        public string Serialize()
        {
            var content =  JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented, IgnoreNullJsonSettings);
            return content;
        }
    }
}
