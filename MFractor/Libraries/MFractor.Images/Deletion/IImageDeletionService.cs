using System;
using System.Collections.Generic;
using System.Text;
using MFractor.Work;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Deletion
{
    public interface IImageDeletionService
    {
        IReadOnlyList<IWorkUnit> Delete(IImageAsset imageAsset);

        IReadOnlyList<IWorkUnit> Delete(Project project, IImageAsset imageAsset);
        IReadOnlyList<IWorkUnit> Delete(Project project, IEnumerable<IProjectFile> files);
        IReadOnlyList<IWorkUnit> Delete(IEnumerable<IProjectFile> files);
    }
}
