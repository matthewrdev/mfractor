using System.Collections.Generic;
using MFractor.Versioning;

namespace MFractor
{
    /// <summary>
    /// Inspect the products version to MFractor' subsystems.
    /// </summary>
    public interface IProductInformation
    {
        /// <summary>
        /// The version of the product.
        /// </summary>
        /// <value>The version.</value>
        SemanticVersion Version { get; }

        /// <summary>
        /// The name of the product.
        /// </summary>
        string ProductName { get; }

        /// <summary>
        /// The product variant.
        /// </summary>
        string ProductVariant { get; }

        /// <summary>
        /// The <see cref="SemanticVersion"/> of the external product (such as an IDE) that MFractor is integrated into.
        /// </summary>
        SemanticVersion ExternalProductVersion { get; }

        /// <summary>
        /// The <see cref="Product"/> that MFractor is integrated into.
        /// </summary>
        Product Product { get; }

        /// <summary>
        /// The utm source of this integration.
        /// <para/>
        /// When using the <see cref="IUrlLauncher"/>, the <see cref="UtmSource"/> is automatically added to all opened urls.
        /// </summary>
        string UtmSource { get; }

        /// <summary>
        /// The Stock Keeping Unit, aka, unique product code, of this IDE integeration.
        /// </summary>
        string SKU { get; }

        /// <summary>
        /// If the current release is a major bump, a URL that explains this release and what it adds.
        /// </summary>
        string VersionMarketingUrl { get; }

        /// <summary>
        /// An enumerable of all extensions currently installed into this IDE integration.
        /// </summary>
        IReadOnlyList<string> InstalledExtensions { get; }

        /// <summary>
        /// The .NET runtime version MFractor is executing on.
        /// </summary>
        string RuntimeVersion { get; }
    }
}
