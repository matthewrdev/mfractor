using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.Fonts;
using MFractor.Maui.CodeGeneration.Fonts;
using MFractor.Ide.Commands;
using MFractor.Licensing;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.Maui.Commands.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [RequiresLicense]
    [Export(typeof(CopyFontFamilyXamlToClipboardCommand))]
    class CopyFontFamilyXamlToClipboardCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IFontFamilyOnPlatformGenerator> fontFamilyOnPlatformGenerator;
        public IFontFamilyOnPlatformGenerator FontFamilyOnPlatformGenerator => fontFamilyOnPlatformGenerator.Value;

        readonly Lazy<IClipboard> clipboard;
        public IClipboard Clipboard => clipboard.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IFontService> fontService;
        public IFontService FontService => fontService.Value;

        public string AnalyticsEvent => "Copy FontFamily XAML To Clipboard";

        [ImportingConstructor]
        public CopyFontFamilyXamlToClipboardCommand(Lazy<IFontFamilyOnPlatformGenerator> fontFamilyOnPlatformGenerator,
                                                    Lazy<IClipboard> clipboard,
                                                    Lazy<IDialogsService> dialogsService,
                                                    Lazy<IFontService> fontService,
                                                    Lazy<IWorkEngine> workEngine,
                                                    Lazy<ILicensingService> licensingService)
        {
            this.fontFamilyOnPlatformGenerator = fontFamilyOnPlatformGenerator;
            this.clipboard = clipboard;
            this.dialogsService = dialogsService;
            this.fontService = fontService;
        }

        public void Execute(ICommandContext commandContext)
        {
            var solutionPadContext = commandContext as ISolutionPadCommandContext;
            var projectFile = solutionPadContext.SelectedItem as IProjectFile;

            var font = FontService.GetFont(projectFile.FilePath);

            Clipboard.Text = FontFamilyOnPlatformGenerator.GenerateXaml(font, font.PostscriptName, new List<PlatformFramework>() { PlatformFramework.Android, PlatformFramework.iOS } );

            DialogsService.StatusBarMessage($"The FontFamily XAML for {font.Name} has been copied to the clipboard");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is IProjectFile projectFile)
            {
                var font = FontService.GetFont(projectFile.FilePath);

                if (font != null)
                {
                    return new CommandState(true, true, "Copy FontFamily XAML To Clipboard", "Generates the XAML code to use the selected font");
                }
            }

            return default;
        }
    }
}
