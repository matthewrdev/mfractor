using System;

namespace MFractor.Licensing
{
    public class ProductLicensingInfo
    {
        public string ProductSku { get; }
        public byte[] PublicKey { get; }
        public byte[] LegacyKey { get; }

        public ProductLicensingInfo(string productSku, byte[] publicKey, byte[] legacyKey)
        {
            ProductSku = productSku;
            PublicKey = publicKey;
            LegacyKey = legacyKey;
        }
    }
    
}
