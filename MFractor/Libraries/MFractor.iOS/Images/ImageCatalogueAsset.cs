using System;

namespace MFractor.iOS.Images
{
    public class ImageCatalogueAsset
    {
        public ImageSize Size { get; set; }

        public Idiom Idiom { get; set; }

        public string File { get; set; } 

        public string Scale { get; set; }

        public string Role { get; set; }

        public string SubType { get; set; }
    }
}
