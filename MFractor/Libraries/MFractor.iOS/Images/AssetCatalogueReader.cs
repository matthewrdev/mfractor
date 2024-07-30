using System;
using System.ComponentModel.Composition;

namespace MFractor.iOS.Images
{
    [Export(typeof(IAssetCatalogueReader))]
    class AssetCatalogueReader : IAssetCatalogueReader
    {
        public MFractor.iOS.Images.AssetCatalogue Read(System.IO.FileInfo asssetCatalogFile)
        {
            return null;
        }

        public MFractor.iOS.Images.AssetCatalogue Read(System.IO.Stream stream)
        {
            return null;
        }

        public MFractor.iOS.Images.AssetCatalogue Read(string contents)
        {
            return null;
        }
    }
}