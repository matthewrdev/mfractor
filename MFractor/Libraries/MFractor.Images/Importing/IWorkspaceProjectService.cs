using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Importing
{
    public interface IWorkspaceProjectService
    {
        Task<bool> AddProjectFilesAsync(Project project, IEnumerable<CreateProjectFileWorkUnit> workUnits);

        Task DeleteFileAsync(Project project, string filePath);

        Task DeleteFilesAsync(Project project, params string[] filePaths);

        bool IsFileExistsInProject(Project project, string filePath);
    }
}
