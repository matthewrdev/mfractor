using System;
using System.ComponentModel.Composition;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IApiConfig))]
    class ApiConfig : IApiConfig
    {
        public string ApiKey => "REDACTED";

        public string Endpoint => "https://api.mfractor.com";
    }
}