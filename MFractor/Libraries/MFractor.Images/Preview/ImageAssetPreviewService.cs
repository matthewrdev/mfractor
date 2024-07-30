using System;
using System.Threading.Tasks;
using MFractor.Workspace;

namespace MFractor.Images.Preview
{

    class ImageAssetPreviewService : IImageAssetPreviewService
    {
        public Task<ImageAssetPreview> GetImageAssetPreview(IImageAsset imageAsset)
        {
            throw new NotImplementedException();
        }

        public Task<ImageAssetPreview> GetImageAssetPreview(IProjectFile projectFile)
        {
            throw new NotImplementedException();
        }

        public Task<ImageAssetPreview> GetImageAssetPreview(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}