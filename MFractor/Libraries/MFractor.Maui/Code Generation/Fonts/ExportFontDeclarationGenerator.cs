using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MFractor.Maui.CodeGeneration.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IExportFontDeclarationGenerator))]
    class ExportFontDeclarationGenerator : CodeGenerator, IExportFontDeclarationGenerator
    {
        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        [CodeSnippetArgument("font", "The name of the font asset including file extension")]
        [CodeSnippetResource("Resources/Snippets/ExportFont.txt")]
        [ExportProperty("What is the code snippet to use when creating the export font declaration?")]
        public ICodeSnippet Snippet { get; set; } = null;

        [ExportProperty("The default file where new export font declarations should be added.")]
        public string FontDeclarationFile { get; set; } = "Properties/ExportedFonts.cs";

        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.export_font_declaration";

        public override string Name => "Export Font Declaration Generator";

        public override string Documentation => "Generates a new font export declaration.";

        [ImportingConstructor]
        public ExportFontDeclarationGenerator(Lazy<ITextProviderService> textProviderService)
        {
            this.textProviderService = textProviderService;
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project, string fontAssetFileName)
        {
            var fileName = Path.GetFileName(FontDeclarationFile);

            var exportedFonts = project.Documents.FirstOrDefault(d => Path.GetFileName(d.FilePath) == fileName);

            var code = GenerateCode(fontAssetFileName);

            if (exportedFonts != null)
            {
                // TODO: Inspect syntax tree and inject correctly.
                var content = TextProviderService.GetTextProvider(exportedFonts.FilePath);

                return new InsertTextWorkUnit(Environment.NewLine + code, content.GetText().Length, exportedFonts.FilePath).AsList();
            }

            return new CreateProjectFileWorkUnit()
            {
                FilePath = FontDeclarationFile,
                FileContent = $@"using System;{Environment.NewLine}{Environment.NewLine}{code}",
                TargetProject = project
            }.AsList();
        }

        public string GenerateCode(string fontAssetFileName)
        {
            return Snippet.SetArgumentValue("$font$", fontAssetFileName)
                           .ToString();
        }

        public SyntaxNode GenerateSyntax(string fontAssetFileName)
        {
            var code = GenerateCode(fontAssetFileName);

            return SyntaxFactory.ParseStatement(code);
        }
    }
}