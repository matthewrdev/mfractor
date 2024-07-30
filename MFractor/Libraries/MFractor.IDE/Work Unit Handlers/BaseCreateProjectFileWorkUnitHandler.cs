using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.WorkUnitHandlers
{
    abstract class BaseCreateProjectFileWorkUnitHandler : WorkUnitHandler<CreateProjectFileWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IFileCreationPostProcessorRepository> fileCreationPostProcessors;
        protected IFileCreationPostProcessorRepository FileCreationPostProcessors => fileCreationPostProcessors.Value;

        readonly Lazy<IDialogsService> dialogsService;
        protected IDialogsService DialogsService => dialogsService.Value;

        protected BaseCreateProjectFileWorkUnitHandler(Lazy<IFileCreationPostProcessorRepository> fileCreationPostProcessors,
                                                       Lazy<IDialogsService> dialogsService)
        {
            this.fileCreationPostProcessors = fileCreationPostProcessors;
            this.dialogsService = dialogsService;
        }

        public sealed override async Task<IWorkExecutionResult> OnExecute(CreateProjectFileWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var project = workUnit.TargetProject;

            if (project == null)
            {
                log?.Warning("No project was provided to the " + GetType().Name + ", cannot create the file.");
                return null;
            }

            var diskPath = await AddToProject(workUnit, project);

            if (string.IsNullOrEmpty(diskPath))
            {
                return default;
            }

            var contents = workUnit.FileContent;

            if (workUnit.AllowPostProcessing)
            {
                contents = PostProcess(contents, diskPath, workUnit.TargetProject);
            }

            var result = new WorkExecutionResult();
            result.AddChangedFile(workUnit.FilePath);
            result.AddAppliedWorkUnit(workUnit);
            if (!workUnit.IsBinary)
            {
                result.AddFileCreation(new FileCreation()
                {
                    FilePath = diskPath,
                    Contents = contents
                });
            }
            result.AddProjectToSave(workUnit.TargetProjectIdentifier);

            return result;
        }

        protected async Task<string> AddToProject(CreateProjectFileWorkUnit workUnit, Microsoft.CodeAnalysis.Project project)
        {
            var filePath = workUnit.FilePath;

            var virtualPath = VirtualFilePathHelper.GenerateNonConflictingVirtualFilePath(project, filePath);

            var overwrite = false;

            var createFile = workUnit as CreateProjectFileWorkUnit;
            if (createFile != null && createFile.ShouldOverWrite)
            {
                overwrite = createFile.ShouldOverWrite;
            }

            var projectFolder = new FileInfo(project.FilePath).Directory.FullName;

            var outputPath = VirtualFilePathHelper.VirtualProjectPathToDiskPath(project, filePath);
            if (File.Exists(outputPath) && !overwrite)
            {
                var result = DialogsService.AskQuestion("A file already exists at:\n" + outputPath + "\n\nWould you like to overwrite it?", "Overwrite", "Skip", "Create New File");

                if (result == "Skip")
                {
                    return string.Empty;
                }
                if (result == "Overwrite")
                {
                    overwrite = true;
                }
            }

            if (overwrite)
            {
                virtualPath = workUnit.FilePath;
            }

            virtualPath = PathHelper.CorrectDirectorySeparatorsInPath(virtualPath);

            var directories = virtualPath.Split(Path.DirectorySeparatorChar).ToList();

            if (directories.Count > 1)
            {
                directories.RemoveAt(directories.Count - 1);

                var virtualDirectoryPath = "";
                foreach (var dir in directories)
                {
                    if (string.IsNullOrEmpty(virtualDirectoryPath))
                    {
                        virtualDirectoryPath = dir;
                    }
                    else
                    {
                        virtualDirectoryPath = string.Join(Path.DirectorySeparatorChar.ToString(), virtualDirectoryPath, dir);
                    }

                    var fullDirectoryPath = Path.Combine(projectFolder, virtualDirectoryPath);
                    if (!Directory.Exists(fullDirectoryPath))
                    {
                        Directory.CreateDirectory(fullDirectoryPath);
                    }

                    if (workUnit.ShouldAddFoldersToMsBuild)
                    {
                        await AddVirtualDirectory(project, virtualDirectoryPath);
                    }
                }
            }

            var diskPath = Path.Combine(projectFolder, virtualPath);

            WriteProjectFile(diskPath, workUnit);

            AddMSBuildEntry(diskPath, project, workUnit);

            return diskPath;
        }

        protected void WriteProjectFile(string filePath, CreateProjectFileWorkUnit workUnit)
        {
            try
            {
                if (workUnit.IsBinary)
                {
                    if (workUnit.WriteContentAction != null)
                    {
                        using (var result = File.OpenWrite(filePath))
                        {
                            workUnit.WriteContentAction(result);
                        }
                    }
                }
                else
                {
                    var fileContent = workUnit.FileContent;

                    if (workUnit.WriteContentAction != null)
                    {
                        using (var stream = new MemoryStream())
                        {
                            workUnit.WriteContentAction(stream);
                            stream.Seek(0, SeekOrigin.Begin);
                            using (var result = new StreamReader(stream))
                            {
                                fileContent = result.ReadToEnd();
                            }
                        }
                    }

                    if (workUnit.AllowPostProcessing)
                    {
                        fileContent = PostProcess(fileContent, filePath, workUnit.TargetProject);
                    }

                    File.WriteAllText(filePath, fileContent);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                Debugger.Break();
            }
        }

        string PostProcess(string content, string filePath, Project project)
        {
            var fileInfo = new FileInfo(filePath);
            foreach (var processor in FileCreationPostProcessors)
            {
                if (processor.CanPostProcess(fileInfo, project))
                {
                    content = processor.PostProcess(content, fileInfo, project);
                }
            }

            return content;
        }

        protected abstract void AddMSBuildEntry(string diskPath, Microsoft.CodeAnalysis.Project project, CreateProjectFileWorkUnit workUnit);

        protected abstract Task AddVirtualDirectory(Microsoft.CodeAnalysis.Project project, string virtualDirectoryPath);
    }
}

