using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.CSharp
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [PartMetadata("IsDefault", false)]
    [Export(typeof(IViewModelPropertyGenerator))]
    class ViewModelPropertyGenerator : CodeGenerator, IViewModelPropertyGenerator
    {
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The fully qualified type of the new property.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new property.")]
        [CodeSnippetResource("Resources/Snippets/ViewModelProperty.txt")]
        [ExportProperty("What is the code snippet to use when creating the property declaration?")]
        public ICodeSnippet Snippet
        {
            get; set;
        }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The fully qualified type of the new property.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new property.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Value, "The value to assign to the new property.")]
        [CodeSnippetResource("Resources/Snippets/ViewModelPropertyWithValue.txt")]
        [ExportProperty("What is the code snippet to use when creating a property declaration with a default value?")]
        public ICodeSnippet ValueSnippet
        {
            get; set;
        }

        [ExportProperty("When the new view model property is an `IEnumerable`, what is the default type that should be used instead of `IEnumerable`? To use the provided `IEnumerable` type, set this to an empty string.")]
        public string DefaultEnumerableType
        {
            get; set;
        } = "System.Collections.Generic.List<object>";

        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.csharp.view_model_property";

        public override string Name => "View Model Property Generator";

        public override string Documentation => "Generates a property declaration for a ViewModel. By default, this code generator routes to the standard PropertyGenerator. However, specifying a code snippet will cause the code generator to use that instead.";

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(ITypeSymbol propertyType,
                                                                   Accessibility accesibility, 
                                                                   string propertyName, 
                                                                   string propertyValue)
        {
            return GenerateSyntax(propertyType?.ToString() ?? "System.Object", accesibility, propertyName, propertyValue);
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(MemberDeclaration propertyDeclaration)
        {
            return GenerateSyntax(propertyDeclaration.NamedType,
                                  propertyDeclaration.Accessibility,
                                  propertyDeclaration.Name,
                                  null);
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string propertyType, 
                                                            Accessibility accesibility, 
                                                            string propertyName, 
                                                            string propertyValue)
        {
            var snippet = string.IsNullOrEmpty(propertyValue) ? Snippet : ValueSnippet;

            var type = propertyType;
            if (type == "System.Collections.IEnumerable"
                && !string.IsNullOrEmpty(DefaultEnumerableType))
            {
                type = DefaultEnumerableType;
            }

            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Type, type);
            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, propertyName);
            if (string.IsNullOrEmpty(propertyValue) == false)
            {
                if (propertyType.Equals(typeof(String).FullName, StringComparison.OrdinalIgnoreCase)
                    || propertyType.Equals("string", StringComparison.OrdinalIgnoreCase))
                {
                    propertyValue = $"\"{propertyValue}\"";
                }

                snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Value, propertyValue);
            }
            else
            {
                snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Value, $"default({type})");
            }

            return snippet.AsMembersList();
        }
    }
}
