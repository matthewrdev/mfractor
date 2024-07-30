using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export("Default", typeof(IInstancePropertyGenerator))]
    class InstancePropertyGenerator : CSharpCodeGenerator, IInstancePropertyGenerator
    {
        public override string Documentation => "Generates a instance property with a getter and setter.";

        public override string Identifier => "com.mfractor.code_gen.csharp.instance_property";

        public override string Name => "Generate Instance Property";

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The fully qualified type of the new property.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new propety.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Value, "The value to assign to the new property.")]
        [CodeSnippetResource("Resources/Snippets/Property.txt")]
        [ExportProperty("What is the code snippet to use when creating the property declaration?")]
        public ICodeSnippet Snippet { get; set; }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The fully qualified type of the new property.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new propety.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Value, "The value to assign to the new property.")]
        [CodeSnippetResource("Resources/Snippets/ReadOnlyProperty.txt")]
        [ExportProperty("What is the code snippet to use when creating a readonly property declaration?")]
        public ICodeSnippet ReadOnlySnippet { get; set; }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The fully qualified type of the new property.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new propety.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Value, "The value to assign to the new property.")]
        [CodeSnippetResource("Resources/Snippets/WriteOnlyProperty.txt")]
        [ExportProperty("What is the code snippet to use when creating a write only property declaration?")]
        public ICodeSnippet WriteOnlySnippet { get; set; }

        ICodeSnippet GetSnippet(MemberDeclaration declaration)
        {
            if (!declaration.HasGetter && declaration.HasSetter)
            {
                return WriteOnlySnippet;
            }

            if (declaration.HasGetter && !declaration.HasSetter)
            {
                return ReadOnlySnippet;
            }

            return Snippet;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(MemberDeclaration propertyDeclaration)
        {
            return GenerateSyntax(GetSnippet(propertyDeclaration),
                                  propertyDeclaration.NamedType.ToString(),
                                  propertyDeclaration.Accessibility,
                                  propertyDeclaration.Name,
                                  null);
        }

        public virtual IEnumerable<MemberDeclarationSyntax> GenerateSyntax(ITypeSymbol propertyType, 
                                                                    Accessibility accesibility, 
                                                                    string propertyName, 
                                                                    string propertyValue)
        {
            return GenerateSyntax(Snippet, 
                                  propertyType.ToString(),
                                  accesibility,
                                  propertyName, 
                                  propertyValue);
        }


        public virtual IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string propertyType,
                                                                    Accessibility accesibility,
                                                                    string propertyName,
                                                                    string propertyValue)
        {
            return GenerateSyntax(Snippet, propertyType, accesibility, propertyName, propertyValue);
        }

        public virtual IEnumerable<MemberDeclarationSyntax> GenerateSyntax(ICodeSnippet snippet,
                                                                           string propertyType, 
                                                                           Accessibility accesibility, 
                                                                           string propertyName, 
                                                                           string propertyValue)
        {
            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Type, propertyType);
            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, propertyName);
            if (string.IsNullOrEmpty(propertyValue) == false)
            {
                snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Value, propertyValue);
            }
            else
            {
                snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Value, $"default({propertyType})");
            }

            return snippet.AsMembersList();
        }
    }
}
