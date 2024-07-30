using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Ide.Commands;
using MFractor.Images.Utilities;
using MFractor.Images.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.Images.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class DeleteImageAssetCommand : ICommand, IAnalyticsFeature
    {
        public const string Tip = "Use the delete image asset tool to remove an image and all it's sizes from your application.";

        readonly Lazy<IImageAssetService> imageAssetService;
        public IImageAssetService ImageAssetService => imageAssetService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "Delete Image Asset";

        [ImportingConstructor]
        public DeleteImageAssetCommand(Lazy<IImageAssetService> imageAssetService,
                                       Lazy<IWorkEngine> workEngine)
        {
            this.imageAssetService = imageAssetService;
            this.workEngine = workEngine;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var available = false;

            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is IProjectFile file
                && ImageHelper.IsImageFile(file.FilePath))
            {
                available = file.IsAndroidImageAsset() || file.IsIOSImageAsset();
            }

            return new CommandState()
            {
                CanExecute = available,
                IsVisible = available,
                Label = "Delete Image Asset",
                Description = "Launch the image asset deletion tool to remove all variants and densities of this image asset from the solution."
            };
        }

        public void Execute(ICommandContext commandContext)
        {
            var file = (commandContext as ISolutionPadCommandContext).SelectedItem as IProjectFile;

            var project = file.CompilationProject;
                
            var imageName = Path.GetFileName(file.FilePath);

            if (project.IsAppleUnifiedProject())
            {
                imageName = ImageNameHelper.GetBundleResourceImageName(imageName);
            }

            var imageAsset = ImageAssetService.FindImageAsset(imageName, project.Solution);

            WorkEngine.ApplyAsync(new DeleteImageAssetWorkUnit()
            {
                ImageAsset = imageAsset,
            });
        }
    }
}
