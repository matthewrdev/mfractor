using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;

namespace MFractor.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontAssetCache))]
    class FontAssetCache : IFontAssetCache, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IApplicationPaths> applicationPaths;
        public IApplicationPaths ApplicationPaths => applicationPaths.Value;

        readonly Lazy<IFontService> fontService;
        public IFontService FontService => fontService.Value;

        readonly Lazy<ISharedHttpClient> sharedHttpClient;
        public ISharedHttpClient SharedHttpClient => sharedHttpClient.Value;

        [ImportingConstructor]
        public FontAssetCache(Lazy<IApplicationPaths> applicationPaths,
                              Lazy<IFontService> fontService,
                              Lazy<ISharedHttpClient> sharedHttpClient)
        {
            this.applicationPaths = applicationPaths;
            this.fontService = fontService;
            this.sharedHttpClient = sharedHttpClient;
        }

        private string CacheFolder => Path.Combine(ApplicationPaths.ApplicationDataPath, "fonts");

        public IFont TryGetFontAsset(string fontAssetUrl)
        {
            if (string.IsNullOrEmpty(fontAssetUrl))
            {
                throw new ArgumentException($"'{nameof(fontAssetUrl)}' cannot be null or empty.", nameof(fontAssetUrl));
            }

            var extension = Path.GetExtension(fontAssetUrl);
            var fontAssetId = GetFontAssetId(fontAssetUrl);
            var filePath = Path.Combine(CacheFolder, fontAssetId + extension);

            if (File.Exists(filePath))
            {
                return FontService.GetFont(filePath);
            }

            DownloadFontAsset(fontAssetUrl, filePath).ConfigureAwait(false);
            return null;
        }

        Task DownloadFontAsset(string fontAssetUrl, string filePath)
        {
            return Task.Run(async () =>
           {
               try
               {
                   var uri = new Uri(fontAssetUrl);
                   var tempFilePath = filePath + ".temp";

                   var response = await SharedHttpClient.HttpClient.GetAsync(uri);
                   using (var fs = new FileStream(filePath, FileMode.CreateNew))
                   {
                       await response.Content.CopyToAsync(fs);
                   }

                   if (File.Exists(tempFilePath))
                   {
                       File.Move(tempFilePath, filePath);
                   }
               }
               catch (Exception ex)
               {
                   log?.Exception(ex);
               }
           });
        }

        string GetFontAssetId(string fontAssetUrl)
        {
            if (string.IsNullOrEmpty(fontAssetUrl))
            {
                throw new ArgumentException($"'{nameof(fontAssetUrl)}' cannot be null or empty.", nameof(fontAssetUrl));
            }

            return MFractor.Utilities.SHA1Helper.FromString(fontAssetUrl);
        }

        public void Startup()
        {
            if (!Directory.Exists(CacheFolder))
            {
                Directory.CreateDirectory(CacheFolder);
            }

            try
            {
                foreach (var tempFiles in Directory.GetFiles(CacheFolder, "*.temp"))
                {
                    File.Delete(tempFiles);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void Shutdown()
        {
        }
    }
}