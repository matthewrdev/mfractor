using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.Fonts;
using MFractor.Ide.Commands;
using MFractor.iOS.Fonts;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.iOS.Commands
{
    [RequiresLicense]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class AddFontPlistEntryCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IFontPlistEntryGenerator> fontPlistEntryGenerator;
        public IFontPlistEntryGenerator FontPlistEntryGenerator => fontPlistEntryGenerator.Value;

        readonly Lazy<IFontService> fontService;
        public IFontService FontService => fontService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        public string AnalyticsEvent => "Add Font PList Entry";

        [ImportingConstructor]
        public AddFontPlistEntryCommand(Lazy<IFontPlistEntryGenerator> fontPlistEntryGenerator,
                                        Lazy<IFontService> fontService,
                                        Lazy<IWorkEngine> workEngine,
                                        Lazy<IDialogsService> dialogsService)
        {
            this.fontPlistEntryGenerator = fontPlistEntryGenerator;
            this.fontService = fontService;
            this.workEngine = workEngine;
            this.dialogsService = dialogsService;
        }

        public void Execute(ICommandContext commandContext)
        {
            var solutionPadContext = commandContext as ISolutionPadCommandContext;
            var projectFile = solutionPadContext.SelectedItem as IProjectFile;

            var font = FontService.GetFont(projectFile.FilePath);

            var plistEntry = FontPlistEntryGenerator.CreateIOSPlistEntry(projectFile.CompilationProject, font.Name);

            if (plistEntry == null)
            {
                DialogsService.ToolbarMessage($"A UIAppFonts entry for {font.Name} already exists in the info.plist");
                return;
            }

            WorkEngine.ApplyAsync(plistEntry);

            DialogsService.ToolbarMessage($"A UIAppFonts entry for {font.Name} has been added to {projectFile.CompilationProject.Name}'s Info.plist");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is IProjectFile projectFile
                && projectFile.CompilationProject.IsAppleUnifiedProject())
            {
                var font = FontService.GetFont(projectFile.FilePath);

                if (font != null)
                {
                    return new CommandState(true, true, "Add To UIAppFonts", "Adds an info.plist entry to the UIAppFonts for a font.");
                }
            }

            return default;
        }
    }
}
