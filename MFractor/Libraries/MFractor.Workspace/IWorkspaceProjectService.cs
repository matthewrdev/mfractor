using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace
{
    public interface IWorkspaceProjectService
    {
        // TODO: Add the methods to make the project workspace service, a service
        //  analogous to the IProjectService but using the IProject interface as the
        //  the base abstraction of a project object. This will allow decoupling
        //  the services provides in a context of a project with loading the rosylin
        //  environment that is generally tied to a Visual Studio instance.

        Task<bool> AddProjectFilesAsync(Project project, IEnumerable<CreateProjectFileWorkUnit> workUnits);

        bool IsProjectFileExists(Project project, string path);

    }
}
