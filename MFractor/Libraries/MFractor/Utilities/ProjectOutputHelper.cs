using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class to retrieve the intermediate and bin output paths for a given <see cref="Project"/>.
    /// </summary>
    public static class ProjectOutputHelper
    {
        public static string GetIntermediateOutputPath(Project project)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var path = new FileInfo(project.FilePath).DirectoryName;
            var output = new FileInfo(project.OutputFilePath).DirectoryName;

            var relative = output.Replace(path + Path.DirectorySeparatorChar, string.Empty);
            var intermediateOutputPath = Path.Combine(path, relative.Replace("bin", "obj"));

            return intermediateOutputPath;
        }

        public static string GetRelativeIntermediateOutputPath(Project project)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var path = new FileInfo(project.FilePath).DirectoryName;
            var output = new FileInfo(project.OutputFilePath).DirectoryName;

            var relative = output.Replace(path + Path.DirectorySeparatorChar, string.Empty);
            return relative.Replace("bin", "obj");
        }

        public static string GetProjectBuildConfiguration(Project project)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var intermediatePath = GetIntermediateOutputPath(project);

            if (string.IsNullOrEmpty(intermediatePath))
            {
                return string.Empty; // ?
            }

            var info = new DirectoryInfo(intermediatePath);

            return info.Name;
        }
    }
}
