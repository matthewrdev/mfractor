using System;
using System.ComponentModel.Composition;

namespace MFractor.iOS.Images
{
    [Export(typeof(IAssetCatalogueWriter))]
    public class AssetCatalogueWriter : IAssetCatalogueWriter
    {
        public bool Write(MFractor.iOS.Images.AssetCatalogue assetCatalog, string filePath)
        {
            return false;
        }

        public bool Write(MFractor.iOS.Images.AssetCatalogue assetCatalog, System.IO.Stream stream)
        {
            return false;
        }

        public string Write(MFractor.iOS.Images.AssetCatalogue assetCatalog)
        {
            return string.Empty;
        }
    }
}