using System;
using MFractor.Images;
using MFractor;
using Microsoft.CodeAnalysis.Text;
using MFractor.Workspace;

namespace MFractor.Images
{
    /// <summary>
    /// An implementation of <see cref="IImageAssetUsage"/> that may be used as a base class.
    /// </summary>
    public class ImageAssetUsage : IImageAssetUsage
    {
        public ImageAssetUsage(ProjectIdentifier projectIdentifier,
                               IProjectFile projectFile,
                               string imageAssetName,
                               string usage,
                               TextSpan span,
                               ImageAssetUsageKind imageAssetUsageKind)
        {
            ProjectIdentifier = projectIdentifier;
            ProjectFile = projectFile;
            ImageAssetName = imageAssetName;
            Usage = usage;
            Span = span;
            ImageAssetUsageKind = imageAssetUsageKind;
        }

        public ProjectIdentifier ProjectIdentifier
        {
            get;
        }

        public IProjectFile ProjectFile
        {
            get;
        }

        public string ImageAssetName
        {
            get;
        }

        public string Usage
        {
            get;
        }

        public TextSpan Span
        {
            get;
        }

        public ImageAssetUsageKind ImageAssetUsageKind { get; }
    }
}