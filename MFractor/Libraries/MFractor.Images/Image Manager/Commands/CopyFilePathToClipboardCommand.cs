using System;
using System.ComponentModel.Composition;
using MFractor.Commands;


namespace MFractor.Images.ImageManager.Commands
{
    class CopyFilePathToClipboardCommand : ImageManagerCommand
    {
        readonly Lazy<IClipboard> clipboard;
        public IClipboard Clipboard => clipboard.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        public override string AnalyticsEvent => "Copy Image FilePath To Clipboard";

        [ImportingConstructor]
        public CopyFilePathToClipboardCommand(Lazy<IClipboard> clipboard,
                                          Lazy<IDialogsService> dialogsService)
        {
            this.clipboard = clipboard;
            this.dialogsService = dialogsService;
        }

        protected override void OnExecute(IImageManagerCommandContext commandContext)
        {
            var projectFile = commandContext.ProjectFile;

            Clipboard.Text = projectFile.FilePath;
            DialogsService.StatusBarMessage($"Copied '{projectFile.FilePath}' to clipboard");
        }

        protected override ICommandState OnGetExecutionState(IImageManagerCommandContext commandContext)
        {
            if (commandContext.ProjectFile == null)
            {
                return default;
            }

            return new CommandState(true, true, "Copy Path To Clipboard", "Copy the file path of this image file to the clipboard");
        }
    }
}
