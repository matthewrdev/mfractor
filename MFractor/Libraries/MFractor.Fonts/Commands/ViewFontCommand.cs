using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Fonts.WorkUnits;
using MFractor.Ide.Commands;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.Fonts.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ViewFontCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IFontService> fontService;
        public IFontService FontService => fontService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "View Font Asset";

        [ImportingConstructor]
        public ViewFontCommand(Lazy<IFontService> fontService,
                               Lazy<IWorkEngine> workEngine)
        {
            this.fontService = fontService;
            this.workEngine = workEngine;
        }

        IFont GetFontAsset(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is IProjectFile projectFile)
            {
                return FontService.GetFont(projectFile.FilePath);
            }

            return default;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var fontAsset = GetFontAsset(commandContext);

            var isAvailable = fontAsset != null;

            return new CommandState(isAvailable, isAvailable, "View Font", "Open this font in the Font Viewer pad.");
        }

        public void Execute(ICommandContext commandContext)
        {
            WorkEngine.ApplyAsync(new FontViewerWorkUnit()
            {
                Font = GetFontAsset(commandContext),
            });
        }
    }
}
