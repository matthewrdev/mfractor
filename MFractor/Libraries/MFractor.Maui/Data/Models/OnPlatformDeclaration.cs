using System.Collections.Generic;
using System.Linq;
using MFractor.Utilities;
using MFractor.Workspace.Data.Models;
using Newtonsoft.Json;


namespace MFractor.Maui.Data.Models
{
    public class OnPlatformDeclaration : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The primary key of the <see cref="StaticResourceDefinition"/> that is this OnPlatforms parent.
        /// </summary>
        /// <value>The static resource identifier.</value>
        public int StaticResourceKey { get; set; }

        /// <summary>
        /// The name of this color resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The fully qualified meta-type that this on platform targets.
        /// </summary>
        /// <value>The type of the target.</value>
        public string Type { get; set; }

        /// <summary>
        /// A json then base 64 encoded <see cref="Dictionary{TKey, TValue}"/> of strings for the Platform/Value pair.
        /// </summary>
        public string PlatformsJsonBase64
        {
            get;
            set;
        }

        /// <summary>
        /// The platforms that this OnPlatform targets.
        /// </summary>
        
        public Dictionary<string, string> Platforms
        {
            get
            {

                if (string.IsNullOrEmpty(PlatformsJsonBase64))
                {
                    return new Dictionary<string, string>();
                }

                try
                {
                    var decoded = Base64Helper.Decode(PlatformsJsonBase64);
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(decoded);
                }
                catch { }

                return new Dictionary<string, string>();
            }

            set
            {
                if (value == null)
                {
                    PlatformsJsonBase64 = null;
                }
                else
                {
                    try
                    {
                        PlatformsJsonBase64 = Base64Helper.Encode(JsonConvert.SerializeObject(value));
                    }
                    catch { }
                }
            }
        }

        
        public bool HasPlatforms => Platforms.Any();
    }
}
