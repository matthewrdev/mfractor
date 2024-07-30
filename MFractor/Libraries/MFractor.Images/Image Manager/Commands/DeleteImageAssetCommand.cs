using System;
using System.ComponentModel.Composition;
using MFractor.Commands;
using MFractor.Images.WorkUnits;
using MFractor.Work;

namespace MFractor.Images.ImageManager.Commands
{
    class DeleteImageAssetCommand : ImageManagerCommand
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public DeleteImageAssetCommand(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        public override string AnalyticsEvent => "Delete Image Asset Command";

        protected override void OnExecute(IImageManagerCommandContext commandContext)
        {
            var workUnit = new DeleteImageAssetWorkUnit()
            {
                OnImagesDeleted = (images) =>
                {
                    commandContext.ImageManagerController.GatherImageAssetsAsync().ConfigureAwait(false);
                },
                ImageAsset = commandContext.ImageAsset
            };

            WorkEngine.ApplyAsync(workUnit);
        }

        protected override ICommandState OnGetExecutionState(IImageManagerCommandContext commandContext)
        {
            if (!commandContext.ImageManagerOptions.AllowDelete
                || commandContext.ImageAsset == null)
            {
                return default;
            }

            return new CommandState(true, true, "Delete", "Delete the image asset from this solution");
        }
    }
}
