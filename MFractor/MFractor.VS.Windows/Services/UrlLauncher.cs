
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Web;
using ExportAttribute = System.ComponentModel.Composition.ExportAttribute;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IUrlLauncher))]
    class UrlLauncher : IUrlLauncher
    {
        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public UrlLauncher(Lazy<IProductInformation> productInformation,
                           Lazy<IDispatcher> dispatcher)
        {
            this.productInformation = productInformation;
            this.dispatcher = dispatcher;
        }

        public void OpenUrl(string url, bool addUtmSource = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (addUtmSource)
            {
                url = AddUtmSource(url);
            }

            Process.Start(url);
        }

        public void OpenUrl(Uri uri, bool addUtmSource = true)
        {
            if (uri == null)
            {
                return;
            }

            OpenUrl(uri.ToString(), addUtmSource);
        }

        string AddUtmSource(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("message", nameof(url));
            }

            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["utm_source"] = ProductInformation.UtmSource;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }
    }
}