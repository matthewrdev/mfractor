using MFractor.Workspace;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Images
{
    public interface IImageAssetUsage
    {
        /// <summary>
        /// The project identifier where the image asset was used.
        /// </summary>
        ProjectIdentifier ProjectIdentifier { get; }

        /// <summary>
        /// The project file where the image asset was used.
        /// </summary>
        IProjectFile ProjectFile { get; }

        /// <summary>
        /// The name of the image asset that was used.
        /// </summary>
        string ImageAssetName { get; }

        /// <summary>
        /// The usage text/expression for this image asset.
        /// <para/>
        /// For example, in Android AXML this could be "@drawable/my_image".
        /// </summary>
        string Usage { get; }

        /// <summary>
        /// The span of the image asset usage, aka, the text location of the <see cref="Usage"/>.
        /// </summary>
        TextSpan Span { get; }

        /// <summary>
        /// The kind of usage of this asset.
        /// </summary>
        ImageAssetUsageKind ImageAssetUsageKind {get;}
    }
}
