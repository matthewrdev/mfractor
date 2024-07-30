using System;
using System.IO;

namespace MFractor.iOS.Images
{
    public interface IAssetCatalogueReader
    {
        AssetCatalogue Read(FileInfo asssetCatalogFile);
        AssetCatalogue Read(Stream stream);
        AssetCatalogue Read(string contents);
    }
}
