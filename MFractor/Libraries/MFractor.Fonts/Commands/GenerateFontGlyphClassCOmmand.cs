using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.Fonts.CodeGeneration;
using MFractor.Ide.Commands;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;

namespace MFractor.Fonts.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    [RequiresLicense]
    class GenerateFontGlyphClassCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IFontService> fontService;
        public IFontService FontService => fontService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IFontCharacterCodeClassGenerator> fontCharacterCodeClassGenerator;
        public IFontCharacterCodeClassGenerator FontCharacterCodeClassGenerator => fontCharacterCodeClassGenerator.Value;

        public string AnalyticsEvent => "Generate Font Character Code Class Command";

        [ImportingConstructor]
        public GenerateFontGlyphClassCommand(Lazy<IFontService> fontService,
                                             Lazy<IWorkEngine> workEngine,
                                             Lazy<IFontCharacterCodeClassGenerator> fontCharacterCodeClassGenerator)
        {
            this.fontService = fontService;
            this.workEngine = workEngine;
            this.fontCharacterCodeClassGenerator = fontCharacterCodeClassGenerator;
        }

        public void Execute(ICommandContext commandContext)
        {
            var solutionPadContext = commandContext as ISolutionPadCommandContext;
            var projectFile = solutionPadContext.SelectedItem as IProjectFile;

            var font = FontService.GetFont(projectFile.FilePath);

            IReadOnlyList<IWorkUnit> generateCodeFiles(GenerateCodeFilesResult result)
            {
                return FontCharacterCodeClassGenerator.Generate(font, result.Name, result.FolderPath, result.SelectedProject);
            }

            WorkEngine.ApplyAsync(new GenerateCodeFilesWorkUnit(CSharpNameHelper.ConvertToValidCSharpName(font.PostscriptName),
                                                                projectFile.CompilationProject,
                                                                projectFile.CompilationProject.Solution.Projects,
                                                                "Fonts",
                                                                "Generate Font Character Code Class",
                                                                "Generate a new class with named constants for all character codes in this font file",
                                                                string.Empty,
                                                                ProjectSelectorMode.Single,
                                                                generateCodeFiles));
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is IProjectFile projectFile)
            {
                var font = FontService.GetFont(projectFile.FilePath);

                if (font != null)
                {
                    return new CommandState(true, true, "Generate Font Character Code Class", "Generate a new class with named constants for all character codes in this font file");
                }
            }

            return default;
        }
    }
}