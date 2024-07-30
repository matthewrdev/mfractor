using System;
using System.Collections.Generic;
using System.IO;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.Documents;
using MFractor.Images;

namespace MFractor.Maui.Analysis
{
    /// <summary>
    /// A <see cref="ICodeAnalysisPreprocessor"/> that gathers the available images for a project and caches them.
    /// <para/>
    /// The <see cref="ImageResourceAnalysisPreprocessor"/> does not gather any image assets until it is used in an analyser.
    /// </summary>
    public class ImageResourceAnalysisPreprocessor : ICodeAnalysisPreprocessor
    {
        readonly IImageAssetService imageAssetService;

        public ImageResourceAnalysisPreprocessor(IImageAssetService imageAssetService)
        {
            this.imageAssetService = imageAssetService;
        }

        public IImageAsset GetImageAssetByName(string imageName)
        {
            if (string.IsNullOrEmpty(imageName) || ImageAssets == null)
            {
                return default;
            }

            var name = Path.GetFileNameWithoutExtension(imageName);

            if (!ImageAssets.ContainsKey(name))
            {
                return default;
            }

            return ImageAssets[name];
        }

        Lazy<IReadOnlyDictionary<string, IImageAsset>> imageAssets;
        public IReadOnlyDictionary<string, IImageAsset> ImageAssets => imageAssets.Value;

        public bool IsValid { get; private set; } = false;

        public bool Preprocess(IParsedXmlDocument document, IFeatureContext context)
        {
            var xamlContext = context as IXamlFeatureContext;
            var xamlDocument = document as IParsedXamlDocument;

            if (xamlContext == null || xamlDocument == null)
            {
                return false;
            }

            IsValid = true;

            imageAssets = new Lazy<IReadOnlyDictionary<string, IImageAsset>>(() =>
           {
               return imageAssetService.GatherImageAssets(context.Project, true);
           });

            return true;
        }
    }
}
