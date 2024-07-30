using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Images.Settings;
using MFractor.Tinify;
using MFractor.Utilities;
using MFractor.Workspace;

namespace MFractor.Images.Optimisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageOptimisationService))]
    class ImageOptimisationService : IImageOptimisationService, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        readonly Lazy<IImageFeatureSettings> imageFeatureSettings;
        public IImageFeatureSettings ImageFeatureSettings => imageFeatureSettings.Value;

        [ImportingConstructor]
        public ImageOptimisationService(Lazy<IProductInformation> productInformation,
                                        Lazy<IImageFeatureSettings> imageFeatureSettings)
        {
            this.productInformation = productInformation;
            this.imageFeatureSettings = imageFeatureSettings;
        }

        public void SetApiKey(string apiKey)
        {
            try
            {
                TinifyClient.AppIdentifier = ProductInformation.SKU;
                TinifyClient.Key = apiKey;
                TinifyClient.ValidateServerCertificate = false;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public bool HasApiKey => !string.IsNullOrEmpty(ImageFeatureSettings.TinyPNGApiKey);

        public OptimisationResult Optimise(string filePath, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            return OptimiseAsync(filePath, progressMessageCallback, cancellation).Result;
        }

        public async Task<OptimisationResult> OptimiseAsync(string filePath, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    return OptimisationResult.CreateFailure(filePath, "Does not exist");
                }

                if (!ImageHelper.IsImageFile(filePath))
                {
                    return OptimisationResult.CreateFailure(filePath, "The file is not an image file.");
                }

                log?.Info("Optimising: " + filePath);

                var sizeBefore = FileSizeHelper.GetFileSize(filePath);

                progressMessageCallback?.Invoke("Shrinking " + filePath);

                var source = await TinifyClient.FromFile(filePath);

                cancellation.ThrowIfCancellationRequested();

                await source.ToFile(filePath);

                var sizeAfter = FileSizeHelper.GetFileSize(filePath);

                return new OptimisationResult(filePath, true, sizeBefore, sizeAfter);
            }
            catch (TinifyException tex)
            {
                log?.Info("Optimisation failed: " + tex.ToString());
                return OptimisationResult.CreateFailure(filePath, tex);
            }
            catch (OperationCanceledException)
            {
                return OptimisationResult.CreateFailure(filePath, "Optimisation was cancelled");
            }
            catch (System.Exception ex)
            {
                log?.Info("Optimisation failed: " + ex.ToString());
                return OptimisationResult.CreateFailure(filePath, ex);
            }
        }

        public OptimisationResult Optimise(IProjectFile projectFile, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            return OptimiseAsync(projectFile, progressMessageCallback, cancellation).Result;
        }

        public IReadOnlyList<OptimisationResult> Optimise(IEnumerable<IProjectFile> projectFiles, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            var results = new List<OptimisationResult>();

            foreach (var pf in projectFiles)
            {
                if (!ImageHelper.IsImageFileExtension(pf.Extension))
                {
                    log?.Info(pf.FilePath + " is not an image file. Skipping optimisation.");
                    continue;
                }

                try
                {
                    var result = Optimise(pf, progressMessageCallback, cancellation);
                    results.Add(result);
                }
                catch (OperationCanceledException)
                {
                    results.Add(OptimisationResult.CreateFailure(pf, "The optimisation was cancelled"));
                    break;
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                    results.Add(OptimisationResult.CreateFailure(pf, ex));
                }
            }

            return results;
        }

        public IReadOnlyList<OptimisationResult> Optimise(IImageAsset imageAsset, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            var results = new List<OptimisationResult>();

            foreach (var project in imageAsset.Projects)
            {
                try
                {
                    results.AddRange(Optimise(imageAsset.GetAssetsFor(project), progressMessageCallback, cancellation));
                }
                catch (System.Exception ex)
                {
                    log?.Exception(ex);
                }
            }
            return results;
        }

        public async Task<OptimisationResult> OptimiseAsync(IProjectFile projectFile, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            try
            {
                if (projectFile == null || !File.Exists(projectFile.FilePath))
                {
                    return OptimisationResult.CreateFailure(projectFile);
                }

                var sizeBefore = FileSizeHelper.GetFileSize(projectFile.FilePath);

                progressMessageCallback?.Invoke("Shrinking " + projectFile.CompilationProject.Name + "/" + projectFile.VirtualPath);

                var source = await TinifyClient.FromFile(projectFile.FilePath);

                cancellation.ThrowIfCancellationRequested();

                await source.ToFile(projectFile.FilePath);

                var sizeAfter = FileSizeHelper.GetFileSize(projectFile.FilePath);

                return new OptimisationResult(projectFile, true, sizeBefore, sizeAfter);
            }
            catch (TinifyException tex)
            {
                return OptimisationResult.CreateFailure(projectFile, tex);
            }
            catch (OperationCanceledException)
            {
                return OptimisationResult.CreateFailure(projectFile, "Optimisation was cancelled");
            }
            catch (System.Exception ex)
            {
                return OptimisationResult.CreateFailure(projectFile, ex);
            }
        }

        public Task<IReadOnlyList<OptimisationResult>> OptimiseAsync(IEnumerable<IProjectFile> projectFiles, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            return Task.Run(() => Optimise(projectFiles, progressMessageCallback, cancellation));
        }

        public Task<IReadOnlyList<OptimisationResult>> OptimiseAsync(IImageAsset imageAsset, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            return Task.Run(() => Optimise(imageAsset, progressMessageCallback, cancellation));
        }

        public Task<IReadOnlyList<OptimisationResult>> OptimiseAsync(IEnumerable<IImageAsset> imageAssets, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            return Task.Run(() => Optimise(imageAssets, progressMessageCallback, cancellation));
        }

        public IReadOnlyList<OptimisationResult> Optimise(IEnumerable<IImageAsset> imageAssets, Action<string> progressMessageCallback, CancellationToken cancellation)
        {
            var results = new List<OptimisationResult>();

            foreach (var asset in imageAssets)
            {
                try
                {
                    results.AddRange(Optimise(asset, progressMessageCallback, cancellation));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (System.Exception ex)
                {
                    log?.Exception(ex);
                }
            }
            return results;
        }

        public void Startup()
        {
            SetApiKey(ImageFeatureSettings.TinyPNGApiKey);
        }

        public void Shutdown()
        {
        }
    }
}