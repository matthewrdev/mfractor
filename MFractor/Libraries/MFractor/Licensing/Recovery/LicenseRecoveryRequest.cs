using System;
using Newtonsoft.Json;

namespace MFractor.Licensing.Recovery
{
    class LicenseRecoveryRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
