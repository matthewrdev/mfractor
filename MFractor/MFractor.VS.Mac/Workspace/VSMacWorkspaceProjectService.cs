using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using MFractor.Images.Importing;
using MFractor.VS.Mac.Utilities;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using IdeProject = MonoDevelop.Projects.Project;

namespace MFractor.VS.Mac.Workspace
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IWorkspaceProjectService))]
    class VSMacWorkspaceProjectService : IWorkspaceProjectService
    {
        [ImportingConstructor]
        public VSMacWorkspaceProjectService()
        {
        }

        public async Task<bool> AddProjectFilesAsync(Project project, IEnumerable<CreateProjectFileWorkUnit> workUnits)
        {
            var ideProject = project.ToIdeProject();
            foreach (var workUnit in workUnits)
            {
                var diskPath = workUnit.FilePath;
                DeleteFileIfExists(ideProject, diskPath);
                WriteFileContent(workUnit);

                var file = workUnit.HasBuildAction
                    ? ideProject.AddFile(diskPath, workUnit.BuildAction)
                    : ideProject.AddFile(diskPath);
                file.Visible = workUnit.Visible;

                if (!string.IsNullOrEmpty(workUnit.BuildAction))
                {
                    file.BuildAction = workUnit.BuildAction;
                }
            }

            await ideProject.SaveAsync(new MonoDevelop.Core.ProgressMonitor());
            return true;
        }

        public bool IsFileExistsInProject(Project project, string filePath) => project.ToIdeProject().GetProjectFile(filePath) != null;

        public async Task DeleteFileAsync(Project project, string filePath)
        {
            var ideProject = project.ToIdeProject();
            DeleteFileIfExists(ideProject, filePath);
            await ideProject.SaveAsync(new MonoDevelop.Core.ProgressMonitor());
        }

        public async Task DeleteFilesAsync(Project project, params string[] filePaths)
        {
            var ideProject = project.ToIdeProject();
            foreach (var filePath in filePaths)
            {
                DeleteFileIfExists(ideProject, filePath);
            }

            await ideProject.SaveAsync(new MonoDevelop.Core.ProgressMonitor());
        }

        void WriteFileContent(CreateProjectFileWorkUnit workUnit)
        {
            var filePath = workUnit.FilePath;
            var file = new FileInfo(filePath);
            file.Directory.Create();

            if (workUnit.IsBinary)
            {
                if (workUnit.WriteContentAction != null)
                {
                    using var result = File.OpenWrite(filePath);
                    workUnit.WriteContentAction(result);
                }
            }
            else
            {
                var fileContent = workUnit.FileContent;
                if (workUnit.WriteContentAction != null)
                {
                    using var stream = new MemoryStream();
                    workUnit.WriteContentAction(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    using var result = new StreamReader(stream);
                    fileContent = result.ReadToEnd();
                }

                File.WriteAllText(filePath, fileContent);
            }
        }

        void DeleteFileIfExists(IdeProject project, string filePath)
        {
            var projectFile = project.GetProjectFile(filePath);
            if (projectFile != null)
            {
                project.Files.Remove(projectFile);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
