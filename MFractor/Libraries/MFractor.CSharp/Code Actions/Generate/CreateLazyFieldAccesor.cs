using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.CodeGeneration;
using MFractor.Code.Documents;
using MFractor.Code.WorkUnits;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeActions.Generate
{
    class CreateLazyFieldAccesor : CSharpCodeAction
    {
        [ExportProperty("The code snippet that creates a public getter property that routes to a lazy field.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The type of the property and field.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Property, "The name of the new property.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Field, "The name of the lazy field.")]
        [CodeSnippetDefaultValue("public $type$ $property$ => $field$.Value;", "The code snippet that creates a public getter property that routes to a lazy field.")]
        public ICodeSnippet CodeSnippet { get; set; }

        public override CodeActionCategory Category => CodeActionCategory.Generate;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.csharp.create_lazy_field_accessor";

        public override string Name => "Create Lazy Field Accessor";

        public override string Documentation => "Generates a property accessor for a lazy field. For example: `IMyService MyService => myLazyService.Value;`";

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return GetFieldAndInnerType(syntax, document, context, out _, out _);
        }

        bool GetFieldAndInnerType(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, out FieldDeclarationSyntax field, out INamedTypeSymbol namedType)
        {
            field = null;
            namedType = null;

            var variable = syntax as VariableDeclaratorSyntax;
            if (variable == null)
            {
                return false;
            }

            field = GetField(variable);
            if (field == null || !field.Declaration.Variables.Any())
            {
                return false;
            }

            if (!context.Project.TryGetCompilation(out var compilation))
            {
                return false;
            }

            var semanticModel = compilation.GetSemanticModel(document.GetSyntaxTree());
            if (semanticModel == null)
            {
                return false;
            }

            var lazyType = semanticModel.GetTypeInfo(field.Declaration.Type).Type as INamedTypeSymbol;
            if (lazyType == null)
            {
                return false;
            }

            if (!lazyType.Name.Equals("Lazy", StringComparison.Ordinal)
                || !lazyType.ContainingNamespace.Name.Equals("System")
                || lazyType.TypeParameters.Length != 1)
            {
                return false;
            }

            namedType = lazyType.TypeArguments[0] as INamedTypeSymbol;

            return namedType != null;
        }

        FieldDeclarationSyntax GetField(VariableDeclaratorSyntax variable)
        {
            SyntaxNode syntax = variable;

            while (syntax.Parent != null)
            {
                syntax = syntax.Parent;

                if (syntax is FieldDeclarationSyntax)
                {
                    return syntax as FieldDeclarationSyntax;
                }
            }

            return null;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Create a property that accesses this lazy fields value").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            GetFieldAndInnerType(syntax, document, context, out var field, out var innerType);

            CodeSnippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Property, GetPropertyName(innerType))
                       .SetArgumentValue(ReservedCodeSnippetArgumentName.Field, field.Declaration.Variables.First().Identifier.Text)
                       .SetArgumentValue(ReservedCodeSnippetArgumentName.Type, innerType.ToString());

            return new InsertSyntaxNodesWorkUnit()
            {
                HostNode = field.Parent,
                AnchorNode = field,
                InsertionLocation = InsertionLocation.End,
                SyntaxNodes = CodeSnippet.AsMembersList().Cast<SyntaxNode>().ToList(),
                Workspace = context.Workspace,
                Project = context.Project,
            }.AsList();
        }

        string GetPropertyName(INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TypeKind == TypeKind.Interface
                && namedTypeSymbol.Name.StartsWith("i", StringComparison.OrdinalIgnoreCase))
            {
                return namedTypeSymbol.Name.Substring(1, namedTypeSymbol.Name.Length - 1).FirstCharToUpper();
            }
            
            return namedTypeSymbol.Name.FirstCharToUpper();
        }
    }
}
