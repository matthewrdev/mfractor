using System;
using System.Threading.Tasks;
using MFractor.Data;
using MFractor.Workspace.Data.Synchronisation;
using MFractor.Text;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;
using MFractor.Workspace;
using MFractor.Workspace.Data;

namespace MFractor.Android.Data
{
    class LayoutSynchroniser : ITextResourceSynchroniser
    {
        public string[] SupportedFileExtensions { get; } = new string[] { ".axml", ".xml" };

        public Task<bool> CanSynchronise(Solution solution, Project project, IProjectFile projectFile)
        {
            var canSync = projectFile.BuildAction == "AndroidResource"
                           && projectFile.ProjectFolders.Count == 2
                           && projectFile.ProjectFolders[0].Equals("Resources", StringComparison.InvariantCultureIgnoreCase)
                           && projectFile.ProjectFolders[1].StartsWith("layout", StringComparison.InvariantCultureIgnoreCase);


            return Task.FromResult(canSync);
        }

        public bool IsAvailable(Solution solution, Project project)
        {
            return project.IsAndroidProject();
        }

        public Task<bool> Synchronise(Solution solution, Project project, IProjectFile projectFile, ITextProvider textProvider, IProjectResourcesDatabase database)
        {
            return Task.FromResult(false);
        }
    }
}
