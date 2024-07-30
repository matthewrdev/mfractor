using System;
using System.Threading.Tasks;
using MFractor.Workspace;

namespace MFractor.Images.Preview
{
    public class ImageAssetPreview
    {
        /// <summary>
        /// The fully qualified path 
        /// </summary>
        public string PreviewFilePath { get; }

        public string Description { get; }
    }

    public interface IImageAssetPreviewService
    {
        Task<ImageAssetPreview> GetImageAssetPreview(IImageAsset imageAsset);

        Task<ImageAssetPreview> GetImageAssetPreview(IProjectFile projectFile);

        Task<ImageAssetPreview> GetImageAssetPreview(string filePath);
    }
}