using MFractor.Images;
using MFractor.Workspace;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Images
{
    class CSharpFormsImageUsage : ImageAssetUsage
    {
        public CSharpFormsImageUsage(ProjectIdentifier projectIdentifier,
                                     IProjectFile projectFile,
                                     string imageAssetName,
                                     string usage,
                                     TextSpan span,
                                     ImageAssetUsageKind imageAssetUsageKind)
            : base(projectIdentifier, projectFile, imageAssetName, usage, span, imageAssetUsageKind)
        {
        }

    }
}
