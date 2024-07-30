using System.Collections.Generic;
using System.IO;
using System.Linq;
using MFractor.IOC;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace.Utilities
{
    /// <summary>
    /// A helper class 
    /// </summary>
    public static class VirtualFilePathHelper
    {
        public static void ExtractVirtualPath(string projectFilePath, string filePath, out string virtualPath, out IReadOnlyList<string> projectFolders)
        {
            virtualPath = string.Empty;
            projectFolders = new List<string>();

            if (string.IsNullOrEmpty(projectFilePath)
                || string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var projectInfo = new FileInfo(projectFilePath);            var relativeFilePath = PathHelper.MakeRelativePath(projectInfo.Directory.FullName, filePath);            if (relativeFilePath.StartsWith(projectInfo.Directory.Name))            {                relativeFilePath = relativeFilePath.Remove(0, projectInfo.Directory.Name.Length);            }            if (relativeFilePath.First() == Path.DirectorySeparatorChar)            {                relativeFilePath = relativeFilePath.Remove(0, 1);            }            virtualPath = relativeFilePath;            relativeFilePath = relativeFilePath.Replace(Path.GetFileName(filePath), "");            if (!string.IsNullOrEmpty(relativeFilePath))            {                if (relativeFilePath.Last() == Path.DirectorySeparatorChar)                {                    relativeFilePath = relativeFilePath.Remove(relativeFilePath.Length - 1);                }                projectFolders = relativeFilePath.Split(Path.DirectorySeparatorChar).ToList();            }            else            {                projectFolders = new List<string>();            }
        }

        public static string GenerateNonConflictingVirtualFilePath(Project project, string virtualFilePath)
        {
            var projectService = Resolver.Resolve<IProjectService>();

            var fileInfo = new FileInfo(virtualFilePath);

            var outputFilePath = virtualFilePath;
            var i = 0;
            var hasFile = true;
            while (hasFile)
            {
                var diskPath = VirtualProjectPathToDiskPath(project, outputFilePath);
                var existingFile = projectService.GetProjectFileWithFilePath(project, diskPath);

                hasFile = existingFile != null;
                if (existingFile != null)
                {
                    i++;
                    var suffix = $" ({i})";
                    var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name) + suffix;

                    if (!string.IsNullOrEmpty(fileInfo.Extension) && !fileInfo.Extension.StartsWith(".", System.StringComparison.Ordinal))
                    {
                        fileName += ".";
                    }

                    fileName += fileInfo.Extension;

                    outputFilePath = Path.Combine(fileInfo.Directory.FullName, fileName);
                }
            }

            return outputFilePath;
        }

        public static string VirtualProjectPathToDiskPath(Microsoft.CodeAnalysis.Project project, string virtualFilePath)
        {
            return Path.Combine(new FileInfo(project.FilePath).Directory.FullName, virtualFilePath);
        }

        public static string ResolveVirtualPath(Project project, IProjectFile file)
        {
            if (project == null || file == null)
            {
                return null;
            }

            if (!file.IsLink)
            {
                return file.FilePath.ToString();
            }

            var projectFileInfo = new FileInfo(project.FilePath);

            var diskPath = Path.Combine(projectFileInfo.Directory.FullName, file.VirtualPath);

            return diskPath;
        }

        public static string ResolveVirtualParentFolder(Project project, IProjectFile file)
        {
            if (project == null || file == null)
            {
                return null;
            }

            var filePath = ResolveVirtualPath(project, file);

            var dir = new FileInfo(filePath).Directory.Name;

            return dir;
        }
    }
}
