using System;
using System.IO;

namespace MFractor.iOS.Images
{
    public interface IAssetCatalogueWriter
    {
        bool Write(AssetCatalogue assetCatalog, string filePath);
        bool Write(AssetCatalogue assetCatalog, Stream stream);
        string Write(AssetCatalogue assetCatalog);
    }
}
