using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.CSharp.CodeGeneration;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Fonts.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontCharacterCodeClassGenerator))]
    class FontCharacterCodeClassGenerator : CodeGenerator, IFontCharacterCodeClassGenerator
    {
        readonly Lazy<IFontService> fontService;
        public IFontService FontService => fontService.Value;

        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.fonts.font_character_code_class";

        public override string Name => "Font Character Code Class Generator";

        public override string Documentation => "Generate a class that contains all font icon codes as public string constants";

        [ImportingConstructor]
        public FontCharacterCodeClassGenerator(Lazy<IFontService> fontService)
        {
            this.fontService = fontService;
        }

        [ExportProperty("The template of a new font character code class.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the character code class.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the character code class lies within.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Members, "The name of the character code class.")]
        [CodeSnippetArgument("font_name", "The name of the font asset that the new class targets.")]
        [CodeSnippetArgument("font_file_names", "The CSV formatted file names of the font assets that the new class targets.")]
        [CodeSnippetResource("Resources/Snippets/FontCharacterCodeClass.txt")]
        public ICodeSnippet Template { get; set; }

        [Import]
        public IFontCharacterCodePropertyGenerator FontCharacterCodePropertyGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        public string GenerateCode(IReadOnlyList<IFont> fonts, string className, string @namespace)
        {
            return GenerateSyntax(fonts, className, @namespace).ToString();
        }

        public CompilationUnitSyntax GenerateSyntax(IReadOnlyList<IFont> fonts, string className, string @namespace)
        {
            var nameValuePairs = new Dictionary<string, string>();

            foreach (var font in fonts)
            {
                var typeface = FontService.GetFontTypeface(font.FilePath);

                foreach (var glyph in typeface.GlyphCollection)
                {
                    if (glyph.CharacterCodeHex.Length != 4)
                    {
                        continue; // Not a glyph.
                    }

                    var propertyName = CSharpNameHelper.ToDotNetName(glyph.Name);

                    if (!nameValuePairs.ContainsKey(propertyName))
                    {
                        nameValuePairs[propertyName] = "\\u" + glyph.CharacterCodeHex;
                    }
                }
            }

            var properties = nameValuePairs.OrderBy(nvp => nvp.Key)
                                           .Select(nvp => FontCharacterCodePropertyGenerator.GenerateCode(nvp.Key, nvp.Value))
                                           .ToList();

            var propertiesBlock = string.Join(Environment.NewLine, properties);

            var fontName = fonts.First().Name;

            var fontsList = fonts.ToList();

            if (fontsList.Count > 1)
            {
                if (fontsList.Count > 2)
                {
                    foreach (var font in fontsList.GetRange(1, fontsList.Count - 2))
                    {
                        fontName += ", " + font.Name;
                    }
                }

                fontName += " and " + fonts.Last().Name + ".";
            }

            var bindings = string.Join(",", fonts.Select(f => f.FileName));

            return Template.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, className)
                           .SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, @namespace)
                           .SetArgumentValue(ReservedCodeSnippetArgumentName.Members, propertiesBlock)
                           .SetArgumentValue("font_name", fontName)
                           .SetArgumentValue("font_file_names", bindings)
                           .AsCompilationUnit();
        }

        public IReadOnlyList<IWorkUnit> Generate(IReadOnlyList<IFont> fonts, string className, string folderPath, Project targetProject)
        {
            var @namespace = NamespaceDeclarationGenerator.GetNamespaceFor(targetProject, folderPath);

            var code = GenerateCode(fonts, className, @namespace);

            var filePath = string.IsNullOrEmpty(folderPath) ? className : Path.Combine(folderPath, className);

            return new CreateProjectFileWorkUnit()
            {
                TargetProject = targetProject,
                FileContent = code,
                FilePath = filePath + ".cs",
            }.AsList();
        }

        public string GenerateCode(IFont font, string className, string @namespace)
        {
            return GenerateCode(font.AsList(), className, @namespace);
        }

        public CompilationUnitSyntax GenerateSyntax(IFont font, string className, string @namespace)
        {
            return GenerateSyntax(font.AsList(), className, @namespace);
        }

        public IReadOnlyList<IWorkUnit> Generate(IFont font, string className, string folderPath, Project targetProject)
        {
            return Generate(font.AsList(), className, folderPath, targetProject);
        }
    }
}