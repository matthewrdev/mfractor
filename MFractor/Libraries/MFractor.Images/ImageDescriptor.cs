using MFractor.IOC;

namespace MFractor.Images
{
    public class ImageDescriptor
    {
        protected readonly IImageUtilities imageUtil;

        public int Width { get; }

        public int Height { get; }

        public ImageSize Size => new ImageSize(Width, Height);

        public string FilePath { get; }

        public ImageDescriptor(string filePath)
        {
            imageUtil = Resolver.Resolve<IImageUtilities>();
            FilePath = filePath;

            var size = imageUtil.GetImageSize(filePath);
            Width = size.Width;
            Height = size.Height;
        }

        public ImageImportDescriptor ToImportDescriptor(ImageDensity desiredDensity, ImportImageOperation downSampling) => new ImageImportDescriptor(FilePath, desiredDensity, downSampling);
    }
}
