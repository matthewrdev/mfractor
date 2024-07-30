using System;
using System.ComponentModel.Composition;
using MFractor.Commands;


namespace MFractor.Images.ImageManager.Commands
{
    class CopyNameToClipboardCommand : ImageManagerCommand
    {
        readonly Lazy<IClipboard> clipboard;
        public IClipboard Clipboard => clipboard.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        public override string AnalyticsEvent => "Copy Image Name To Clipboard";

        [ImportingConstructor]
        public CopyNameToClipboardCommand(Lazy<IClipboard> clipboard,
                                          Lazy<IDialogsService> dialogsService)
        {
            this.clipboard = clipboard;
            this.dialogsService = dialogsService;
        }

        protected override void OnExecute(IImageManagerCommandContext commandContext)
        {
            var asset = commandContext.ImageAsset;

            Clipboard.Text = asset.Name;
            DialogsService.StatusBarMessage($"Copied '{asset.Name}' to clipboard");
        }

        protected override ICommandState OnGetExecutionState(IImageManagerCommandContext commandContext)
        {
            if (commandContext.ImageAsset == null)
            {
                return default;
            }

            return new CommandState(true, true, "Copy Name To Clipboard", "Copy the name of this image asset to the clipboard");
        }
    }
}
