using System;
using System.ComponentModel.Composition;
using MFractor.Commands;

namespace MFractor.Images.ImageManager.Commands
{
    class RevealInFinderCommand : ImageManagerCommand
    {
        readonly Lazy<IOpenFileInBrowserService> openFileInBrowserService;
        public IOpenFileInBrowserService OpenFileInBrowserService => openFileInBrowserService.Value;

        readonly Lazy<IPlatformService> platformService;
        public IPlatformService PlatformService => platformService.Value;

        public override string AnalyticsEvent => "Reveal Image Asset In Finder";

        [ImportingConstructor]
        public RevealInFinderCommand(Lazy<IOpenFileInBrowserService> openFileInBrowserService,
                                    Lazy<IPlatformService> platformService)
        {
            this.openFileInBrowserService = openFileInBrowserService;
            this.platformService = platformService;
        }

        protected override void OnExecute(IImageManagerCommandContext commandContext)
        {
            var projectFile = commandContext.ProjectFile;

            OpenFileInBrowserService.OpenAndSelect(projectFile.FilePath);
        }

        protected override ICommandState OnGetExecutionState(IImageManagerCommandContext commandContext)
        {
            if (commandContext.ProjectFile == null)
            {
                return default;
            }

            var name = PlatformService.IsOsx ? "Finder" : "Explorter";

            return new CommandState(true, true, $"Reveal in {name}", $"Reveals the currently selected file in {name}");
        }
    }
}
