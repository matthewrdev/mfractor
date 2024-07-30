using System;
using System.IO;
using Xwt.Drawing;

namespace MFractor.Views.ImageImporter
{
    public class SourceImage
    {
        public string FilePath { get; }

        public bool Exists => File.Exists(FilePath);

        readonly Lazy<Image> image;
        public Image Image => image.Value;

        public SourceImage(string filePath)
        {
            FilePath = filePath;
            image = new Lazy<Image>(() => Image.FromFile(FilePath));
        }
    }

}
