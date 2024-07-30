using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Images.Optimisation;
using MFractor.Images.WorkUnits;
using MFractor.Images.Settings;
using MFractor.Progress;
using MFractor.Views.Progress;
using Microsoft.CodeAnalysis;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.Views.ImageOptimiser
{
    class OptimiseImageAssetWorkUnitHandler : WorkUnitHandler<OptimiseImageAssetWorkUnit>
    {
        [ImportingConstructor]
        public OptimiseImageAssetWorkUnitHandler(Lazy<IWorkEngine> workEngine,
                                                 Lazy<IRootWindowService> rootWindowService,
                                                 Lazy<IWorkspaceService> workspaceService,
                                                 Lazy<IDialogsService> dialogsService,
                                                 Lazy<IImageFeatureSettings> imageFeatureSettings,
                                                 Lazy<IAnalyticsService> analyticsService,
                                                 Lazy<IImageOptimisationService> imageOptimisationService,
                                                 Lazy<IDispatcher> dispatcher)
        {
            this.workEngine = workEngine;
            this.rootWindowService = rootWindowService;
            this.workspaceService = workspaceService;
            this.dialogsService = dialogsService;
            this.imageFeatureSettings = imageFeatureSettings;
            this.analyticsService = analyticsService;
            this.imageOptimisationService = imageOptimisationService;
            this.dispatcher = dispatcher;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IImageFeatureSettings> imageFeatureSettings;
        public IImageFeatureSettings ImageFeatureSettings => imageFeatureSettings.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IImageOptimisationService> imageOptimisationService;
        public IImageOptimisationService ImageOptimisationService => imageOptimisationService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override Task<IWorkExecutionResult> OnExecute(OptimiseImageAssetWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            if (ValidateApiKey())
            {
                switch (workUnit.ImageAssetOptimisationKind)
                {
                    case ImageAssetOptimisationKind.Bulk:
                        OptimiseBulk(workUnit).ConfigureAwait(false);
                        break;
                    case ImageAssetOptimisationKind.ImageAsset:
                        OptimiseImageAsset(workUnit).ConfigureAwait(false);
                        break;
                    case ImageAssetOptimisationKind.ProjectFile:
                        OptimiseProjectFile(workUnit).ConfigureAwait(false);
                        break;
                }
            }

            return Task.FromResult<IWorkExecutionResult>(default);
        }

        bool ValidateApiKey()
        {
            if (string.IsNullOrEmpty(ImageFeatureSettings.TinyPNGApiKey))
            {
                var message = "MFractor cannot perform image optimisation as an API key for www.tinypng.com has not been provided.\n\nTo obtain an API key, please visit https://tinypng.com/developers and then enter the key into MFractors preferences.";

                var confirmation = DialogsService.Confirm(message, "Get API Key");

                if (confirmation)
                {
                    Process.Start("https://tinypng.com/developers");
                }

                return false;
            }

            return true;
        }

        async Task<bool> OptimiseBulk(OptimiseImageAssetWorkUnit workUnit)
        {
            var imageAssets = workUnit.Assets;

            var affectedProjects = new HashSet<Project>();
            var affectedFiles = new HashSet<IProjectFile>();

            foreach (var asset in imageAssets)
            {
                foreach (var projectFile in asset.AllAssets)
                {
                    affectedProjects.Add(projectFile.CompilationProject);
                    affectedFiles.Add(projectFile);
                }
            }

            var pluralisation = affectedProjects.Count == 1 ? " project" : " projects";

            var filesPluralisation = affectedFiles.Count == 1 ? " file" : " files";

            var confirmation = DialogsService.AskQuestion($"Are you sure that you want to optimise all image assets?\n\nThis will affect " + affectedFiles.Count + filesPluralisation + " in " + affectedProjects.Count + pluralisation + ".", "Yes", "Cancel");
            if (confirmation == "Yes")
            {
                AnalyticsService.Track("Optimise All Image Assets");

                var progressDialog = new ProgressDialog()
                {
                    Title = "Optimising Image Assets",
                };

                progressDialog.Show();

                var results = new List<OptimisationResult>();

                foreach (var imageAsset in imageAssets)
                {
                    var assetCount = imageAsset.AllAssets.Count;

                    var index = 0;

                    progressDialog.SetProgress("Optimising: " + imageAsset.Name, 0);
                    var result = await ImageOptimisationService.OptimiseAsync(imageAsset, (m) =>
                    {
                        Dispatcher.InvokeOnMainThread(() =>
                        {
                            index++;
                            progressDialog.SetProgress(m, index, assetCount);
                        });
                    }, progressDialog.CancellationToken);

                    if (result != null)
                    {
                        results.AddRange(result);
                    }
                }

                progressDialog.Close();
                progressDialog.Dispose();

                var dialog = new ImageOptimisationResultDialog(results);
                dialog.Run(RootWindowService.RootWindowFrame);

                workUnit.OnImageOptimisationFinishedDelegate?.Invoke();
            }

            return true;
        }


        async Task<bool> OptimiseImageAsset(OptimiseImageAssetWorkUnit workUnit)
        {
            var imageAsset = workUnit.ImageAsset;

            var pluralisation = imageAsset.Projects.Count == 1 ? " project" : " projects";

            var count = 0;
            foreach (var p in imageAsset.Projects)
            {
                count += imageAsset.GetAssetsFor(p).Count;
            }

            var filesPluralisation = count == 1 ? " file" : " files";

            var confirmed = true;

            if (workUnit.RequiresConfirmation)
            {

                var confirmation = DialogsService.AskQuestion("Are you sure you want to optimise " + imageAsset.Name + "?\n\nThis will affect " + count + filesPluralisation + " in " + imageAsset.Projects.Count + pluralisation + ".", "Yes", "Cancel");
                confirmed = confirmation == "Yes";
            }

            if (confirmed)
            {
                AnalyticsService.Track("Optimise Image Asset");
                IReadOnlyList<OptimisationResult> result = new List<OptimisationResult>();

                var totalAssets = imageAsset.AllAssets.Count;

                var progressDialog = new ProgressDialog()
                {
                    Title = "Optimising Images",
                };

                progressDialog.SetProgress($"Optimising {totalAssets} image assets...", 0);

                progressDialog.Show();

                var index = 0;
                result = await ImageOptimisationService.OptimiseAsync(imageAsset, (m) =>
                {
                    index++;
                    progressDialog.SetProgress(m, index, totalAssets);
                }, CancellationToken.None);

                progressDialog.Close();
                progressDialog.Dispose();

                var dialog = new ImageOptimisationResultDialog(result);
                dialog.Run();
            }

            return true;
        }

        async Task<bool> OptimiseProjectFile(OptimiseImageAssetWorkUnit workUnit)
        {
            var confirmed = true;

            var projectFile = workUnit.ProjectFile;

            if (workUnit.RequiresConfirmation)
            {

                var confirmation = DialogsService.AskQuestion("Are you sure you want to optimise " + projectFile.Name + "?", "Yes", "Cancel");
                confirmed = confirmation == "Yes";
            }

            if (confirmed)
            {
                AnalyticsService.Track("Optimise Image Asset");
                var result = OptimisationResult.CreateFailure(projectFile, "The image failed to optimise for an unknown reason");

                var progressDialog = new ProgressDialog()
                {
                    Title = "Optimising Images",
                };

                progressDialog.SetProgress($"Optimising " + projectFile.Name, 0);

                progressDialog.Show();

                var index = 0;
                result = await ImageOptimisationService.OptimiseAsync(projectFile, (m) =>
                {
                    index++;
                    progressDialog.SetProgress(m, index, 1);
                }, CancellationToken.None);

                progressDialog.Close();
                progressDialog.Dispose();

                var dialog = new ImageOptimisationResultDialog(result);
                dialog.Run();
            }

            return true;
        }
    }
}
