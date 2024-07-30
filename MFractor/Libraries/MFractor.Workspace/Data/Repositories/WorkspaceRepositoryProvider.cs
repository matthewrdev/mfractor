using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Workspace.Data.Models;

namespace MFractor.Workspace.Data.Repositories
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class WorkspaceRepositoryProvider : IRepositoryCollection
    {
        public void RegisterRepositories(IDatabase database)
        {
            database.RegisterRepository<ProjectFileRepository, ProjectFile>(new ProjectFileRepository());
        }
    }
}
