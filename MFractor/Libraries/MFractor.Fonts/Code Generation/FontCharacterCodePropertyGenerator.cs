using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Fonts.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontCharacterCodePropertyGenerator))]
    class FontCharacterCodePropertyGenerator : CodeGenerator, IFontCharacterCodePropertyGenerator
    {
        [ExportProperty("The template of a new font character code property")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the font icon.")]
        [CodeSnippetArgument("code", "The character code of the font icon.")]
        [CodeSnippetResource("Resources/Snippets/FontCharacterCodeProperty.txt")]
        public ICodeSnippet Template { get; set; }

        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.fonts.font_character_code_property";

        public override string Name => "Font Character Code Property";

        public override string Documentation => "Generates the property to reference a font character code.";

        public string GenerateCode(string name, string code)
        {
            return Template.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, name)
                           .SetArgumentValue("code", code)
                           .ToString();
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string name, string code)
        {
            return Template.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, name)
                           .SetArgumentValue("code", code)
                           .AsMembersList();
        }
    }
}