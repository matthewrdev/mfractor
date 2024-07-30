using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Android
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAndroidManifestResolver))]
    class AndroidManifestResolver : IAndroidManifestResolver
    {
        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public AndroidManifestResolver(Lazy<IProjectService> projectService)
        {
            this.projectService = projectService;
        }

        public IProjectFile ResolveAndroidManifest(Project project)
        {
            if (project is null || !project.IsAndroidProject())
            {
                return default;
            }

            return ProjectService.FindProjectFile(project, (filePath) => Path.GetFileName(filePath) == "AndroidManifest.xml");
        }
    }
}