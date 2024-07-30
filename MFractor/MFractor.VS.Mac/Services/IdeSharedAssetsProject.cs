using System;
using System.IO;
using MFractor;
using MFractor.VS.Mac.Utilities;
using MFractor.Workspace;
using MonoDevelop.Projects.SharedAssetsProjects;

namespace MFractor.VS.Mac.Services
{
    class IdeSharedAssetsProject : ISharedAssetsProject
    {
        readonly SharedAssetsProject sharedAssetsProject;

        public IdeSharedAssetsProject(SharedAssetsProject sharedAssetsProject)
        {
            this.sharedAssetsProject = sharedAssetsProject;

            FilePath = sharedAssetsProject.FileName;
            Guid = SolutionHelper.GetProjectGuid(sharedAssetsProject);
            Name = sharedAssetsProject.Name;

            var fileInfo = new FileInfo(FilePath);

            ProjectItemsFilePath =  Path.Combine(fileInfo.Directory.FullName, Path.GetFileNameWithoutExtension(fileInfo.Name) + ".projitems");
        }

        public string FilePath { get; }

        public string Guid { get; }

        public string Name { get; }

        public string ProjectItemsFilePath { get; }
    }
}