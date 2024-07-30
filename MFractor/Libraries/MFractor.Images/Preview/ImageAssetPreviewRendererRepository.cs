using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Images.Preview
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageAssetPreviewRendererRepository))]
    class ImageAssetPreviewRendererRepository : PartRepository<IImageAssetPreviewRenderer>, IImageAssetPreviewRendererRepository
    {
        [ImportingConstructor]
        public ImageAssetPreviewRendererRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IImageAssetPreviewRenderer> Renderers => Parts;
    }
}