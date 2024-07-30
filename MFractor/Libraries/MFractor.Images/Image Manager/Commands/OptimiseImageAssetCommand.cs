using System;
using System.ComponentModel.Composition;
using MFractor.Commands;
using MFractor.Images.WorkUnits;
using MFractor.Work;

namespace MFractor.Images.ImageManager.Commands
{
    class OptimiseImageAssetCommand : ImageManagerCommand
    {
        public override string AnalyticsEvent => "Optimise Image Asset Command";

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public OptimiseImageAssetCommand(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        protected override void OnExecute(IImageManagerCommandContext commandContext)
        {
            OptimiseImageAssetWorkUnit workUnit = default;
            if (commandContext.ImageAsset != null)
            {
                workUnit = new OptimiseImageAssetWorkUnit(commandContext.ImageAsset)
                {
                    OnImageOptimisationFinishedDelegate = () => commandContext.ImageManagerController.GatherImageAssetsAsync().ConfigureAwait(false)
                };
            }
            else
            {
                workUnit = new OptimiseImageAssetWorkUnit(commandContext.ProjectFile);
            }

            WorkEngine.ApplyAsync(workUnit);
        }

        protected override ICommandState OnGetExecutionState(IImageManagerCommandContext commandContext)
        {
            if (commandContext.ImageAsset != null
                || commandContext.ProjectFile != null)
            {
                return new CommandState(true, true, "Optimise", "Optimise the image asset");
            }

            return default;
        }
    }
}
