using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Maui.CodeGeneration.AttachedProperties
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAttachedPropertyGenerator))]
    class AttachedPropertyGenerator : CodeGenerator, IAttachedPropertyGenerator
    {
        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.csharp.attached_property";

        public override string Documentation => "Generates an attached property implementation.";

        public override string Name => "Generate Attached Property";

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new attached property.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The type of the new attached property.")]
        [CodeSnippetArgument("control_type", "The control type that the new attached property targets.")]
        [CodeSnippetResource("Resources/Snippets/AttachedProperty.txt")]
        [ExportProperty("When creating the new attached property, what is the default code snippet MFractor should use?")]
        public ICodeSnippet Snippet
        {
            get; set;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string propertyName,
                                                            string propertyType,
                                                            string parentType,
                                                            CompilationWorkspace workspace,
                                                            OptionSet formattingOptions)
        {
            return Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, propertyName)
                          .SetArgumentValue(ReservedCodeSnippetArgumentName.Type, propertyType)
                          .SetArgumentValue("control_type", parentType)
                          .AsMembersList();
        }
    }
}
