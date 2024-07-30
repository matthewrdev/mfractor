using System;

namespace MFractor
{
    interface IApiConfig
    {
        string Endpoint { get; }

        string ApiKey { get; }
    }
}