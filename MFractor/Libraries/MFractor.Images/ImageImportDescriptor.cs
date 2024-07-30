using MFractor.Images.Utilities;

namespace MFractor.Images
{

    public class ImageImportDescriptor : ImageDescriptor
    {
        public int ScaledWidth { get; }

        public int ScaledHeight { get; }

        public ImageDensity DesiredDensity { get; set; }

        public string VirtualPath { get; set; }

        public ImageImportDescriptor(string filePath, ImageDensity desiredDensity, ImportImageOperation downSampling) : base(filePath)
        {
            DesiredDensity = desiredDensity;
            VirtualPath = ImageDownsamplingHelper.GetVirtualFilePath(downSampling, desiredDensity, false);
            
            var densitySize = ImageDownsamplingHelper.GetTransformedImageSize(downSampling.SourceSize ?? Size, desiredDensity.Scale, downSampling.SourceDensity.Scale);
            ScaledWidth = densitySize.Width;
            ScaledHeight = densitySize.Height;
        }
    }
}
