using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MFractor.Analytics;
using MFractor.Ide;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MFractor.VS.Mac.Commands.SolutionPad
{
    class CleanAndCompressCommand : CommandHandler, IAnalyticsFeature
    {
        ILicensingService LicensingService => Resolver.Resolve<ILicensingService>();

        IAnalyticsService AnalyticsService => Resolver.Resolve<IAnalyticsService>();

        IProjectService ProjectService => Resolver.Resolve<IProjectService>();

        ISolutionPad SolutionPad => Resolver.Resolve<ISolutionPad>();

        IDialogsService DialogsService => Resolver.Resolve<IDialogsService>();

        IOpenFileInBrowserService OpenFileInBrowserService => Resolver.Resolve<IOpenFileInBrowserService>();

        public string AnalyticsEvent => "Clean And Compress";

        protected override void Run()
        {
            if (!LicensingService.HasActivation)
            {
                return;
            }

            var item = SolutionPad.SelectedItem;


            var progressMonitor = IdeApp.Workbench.ProgressMonitors.GetStatusProgressMonitor("Cleaning bin and obj paths...", Stock.StatusSolutionOperation, false, true, false);

            System.Threading.Tasks.Task.Run(() =>
            {

                FileInfo fileInfo = null;
                try
                {
                    if (item is Solution solution)
                    {
                        fileInfo = new FileInfo(solution.FilePath);
                        Clean(solution);
                    }
                    else if (item is Project project)
                    {
                        fileInfo = new FileInfo(project.FilePath);
                        Clean(project);
                    }
                    else if (item is ProjectIdentifier projectId)
                    {
                        var path = ProjectService.GetProjectPath(projectId);
                        fileInfo = new FileInfo(path);
                        Clean(path);
                    }

                    if (fileInfo == null)
                    {
                        return;
                    }

                    var mfractorWorkingPath = Path.Combine(fileInfo.Directory.FullName, ".mfractor");
                    if (Directory.Exists(mfractorWorkingPath))
                    {
                        Directory.Delete(mfractorWorkingPath, true);
                    }

                    var name = Path.GetFileNameWithoutExtension(fileInfo.Name);

                    var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    var archivePath = GetNonConflictingArchivePath(Path.Combine(desktop, name + ".zip"));

                    var exclusions = new List<string>()
                    {
                        ".git/\\*",
                        "*.DS_Store",
                        "*.userprefs",
                    };

                    if (fileInfo.Extension == ".sln")
                    {
                        exclusions.Add(Path.Combine(fileInfo.Directory.Name, "packages") + "/\\*");
                    }

                    var p = new ProcessStartInfo("zip")
                    {
                        WorkingDirectory = fileInfo.Directory.Parent.FullName,
                        RedirectStandardOutput = false,
                        RedirectStandardInput = true,
                        Arguments = $"-r {archivePath.Replace(" ", "\\ ")} {fileInfo.Directory.Name} -x " + string.Join(" -x ", exclusions),
                        UseShellExecute = false
                    };

                    using (var process = Process.Start(p))
                    {
                        process.WaitForExit();
                    }

                    OpenFileInBrowserService.OpenAndSelect(archivePath);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                finally
                {
                    progressMonitor.ReportSuccess("Compressed " + fileInfo.Name);
                    progressMonitor.Dispose();
                    this.DialogsService.StatusBarMessage("Compressed " + fileInfo.Name);
                    AnalyticsService.Track(this);
                }
            });
        }

        static string GetNonConflictingArchivePath(string archivePath)
        {
            var i = 0;
            var hasFile = true;
            while (hasFile)
            {
                var archiveInfo = new FileInfo(archivePath);
                hasFile = archiveInfo.Exists;
                if (hasFile)
                {
                    i++;
                    var suffix = $" ({i})";
                    var fileName = Path.GetFileNameWithoutExtension(archiveInfo.Name) + suffix + archiveInfo.Extension;
                    archivePath = Path.Combine(archiveInfo.Directory.FullName, fileName);
                }
            }

            return archivePath;
        }

        protected override void Update(CommandInfo info)
        {
            var item = SolutionPad.SelectedItem;

            var available = item is Project || item is Solution || item is ProjectIdentifier;

            info.Enabled = available;
            info.Visible = available;
        }

        public static void Clean(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                Clean(project);
            }
        }

        public static void Clean(Project project)
        {
            Clean(project.FilePath);
        }

        public static void Clean(string projectFilePath)
        {
            try
            {
                var projectFolder = new FileInfo(projectFilePath).Directory.FullName;
                var binPath = Path.Combine(projectFolder, "bin");
                var objPath = Path.Combine(projectFolder, "obj");

                if (Directory.Exists(binPath))
                {
                    Directory.Delete(binPath, true);
                }

                if (Directory.Exists(objPath))
                {
                    Directory.Delete(objPath, true);
                }
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
        }
    }
}
