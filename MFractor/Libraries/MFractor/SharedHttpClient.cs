using System;
using System.ComponentModel.Composition;
using System.Net.Http;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ISharedHttpClient))]
    class SharedHttpClient : ISharedHttpClient, IApplicationLifecycleHandler
    {
        public HttpClient HttpClient { get; private set; } = new HttpClient();

        public void Shutdown()
        {
            HttpClient.Dispose();
            HttpClient = null;
        }

        public void Startup()
        {
        }
    }
}