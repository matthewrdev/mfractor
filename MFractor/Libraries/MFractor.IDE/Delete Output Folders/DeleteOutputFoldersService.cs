using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using MFractor.Progress;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.DeleteOutputFolders
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDeleteOutputFoldersService))]
    class DeleteOutputFoldersService : IDeleteOutputFoldersService
    {
        public void DeleteOutputFolders(Solution solution, IDeleteOutputFoldersOptions options, bool deleteProjectArtifacts, IProgressMonitor progressMonitor)
        {
            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var fileInfo = new FileInfo(solution.FilePath);

            progressMonitor ??= new StubProgressMonitor();

            if (deleteProjectArtifacts)
            {
                foreach (var project in solution.Projects)
                {
                    DeleteOutputFolders(project, options, progressMonitor);
                }
            }

            if (options.DeleteVisualStudioWorkingFolder)
            {
                var vsFolder = Path.Combine(fileInfo.Directory.FullName, ".vs");

                progressMonitor.SetMessage($"Deleting .vs folder for {fileInfo.Name}");
                DeleteDirectoryIfExists(vsFolder);
            }
        }

        public void DeleteOutputFolders(IEnumerable<Project> projects, IDeleteOutputFoldersOptions options, IProgressMonitor progressMonitor)
        {
            if (projects is null)
            {
                throw new ArgumentNullException(nameof(projects));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            progressMonitor ??= new StubProgressMonitor();

            foreach (var project in projects)
            {
                DeleteOutputFolders(project, options, progressMonitor);
            }
        }

        public void DeleteOutputFolders(IReadOnlyDictionary<Project, IDeleteOutputFoldersOptions> projects, IProgressMonitor progressMonitor)
        {
            if (projects is null)
            {
                throw new ArgumentNullException(nameof(projects));
            }

            progressMonitor ??= new StubProgressMonitor();

            foreach (var project in projects)
            {
                DeleteOutputFolders(project.Key, project.Value, progressMonitor);
            }
        }

        public void DeleteOutputFolders(Project project, IDeleteOutputFoldersOptions options, IProgressMonitor progressMonitor)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            progressMonitor ??= new StubProgressMonitor();

            var fileInfo = new FileInfo(project.FilePath);

            if (options.DeleteBin)
            {
                progressMonitor.SetMessage("Deleting bin folder for " + project.Name);
                var directory = Path.Combine(fileInfo.Directory.FullName, "bin");

                DeleteDirectoryIfExists(directory);
            }

            if (options.DeleteObj)
            {
                var objFolder = Path.Combine(fileInfo.Directory.FullName, "obj");
                progressMonitor.SetMessage("Deleting obj folder for " + project.Name);
                if (options.DeletePackages)
                {
                    DeleteDirectoryIfExists(objFolder);
                }
                else if (Directory.Exists(objFolder))
                {
                    var directories = Directory.GetDirectories(objFolder);

                    foreach (var outputDir in directories)
                    {
                        DeleteDirectoryIfExists(outputDir);
                    }
                }
            }
            else
            {
                if (options.DeletePackages)
                {
                    DeleteNugetPackagesCache(project, progressMonitor);
                }
            }
        }

        void DeleteNugetPackagesCache(Project project, IProgressMonitor progressMonitor)
        {
            var fileInfo = new FileInfo(project.FilePath);
            var objFolder = Path.Combine(fileInfo.Directory.FullName, "obj");

            progressMonitor.SetMessage("Deleting nuget cache for " + project.Name);

            var baseName = Path.GetFileName(project.FilePath);

            DeleteFileIfExists(Path.Combine(objFolder, baseName + ".nuget.cache"));
            DeleteFileIfExists(Path.Combine(objFolder, baseName + ".nuget.dgspec.json"));
            DeleteFileIfExists(Path.Combine(objFolder, baseName + ".nuget.g.props"));
            DeleteFileIfExists(Path.Combine(objFolder, baseName + ".nuget.g.targets"));
            DeleteFileIfExists(Path.Combine(objFolder, "project.assets.json"));
        }

        void DeleteDirectoryIfExists(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            if (!Directory.Exists(directory))
            {
                return;
            }

            Directory.Delete(directory, true);
        }

        void DeleteFileIfExists(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            if (!File.Exists(file))
            {
                return;
            }

            File.Delete(file);
        }
    }
}