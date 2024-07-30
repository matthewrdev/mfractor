using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Code.WorkUnits;
using MFractor.CSharp.CodeGeneration;
using MFractor.Utilities;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeActions.Generate
{
    class CreateImmutableConstructor : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Generate;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Documentation => "Use the Create Immutable Constructor refactoring to create a constructor that initializes all readonly members of a class.";

        public override string Identifier => "com.mfractor.code_actions.csharp.create_immutable_constuctor";

        public override string Name => "Create Immutable Constructor";

        [Import]
        public IMemberAssignmentGenerator MemberAssignmentGenerator { get; set; }

        [Import]
        public IConstructorGenerator ConstructorGenerator { get; set; }

        public override bool CanExecute(SyntaxNode syntax,
                                        IParsedCSharpDocument document,
                                        IFeatureContext context,
                                        InteractionLocation location)
        {
            var arguments = GetImmutableMembers(syntax.SyntaxTree, context, location, out _);

            return arguments != null && arguments.Any();
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax,
                                                                  IParsedCSharpDocument document,
                                                                  IFeatureContext context,
                                                                  InteractionLocation location)
        {
            return CreateSuggestion($"Create constructor to initialize all readonly members").AsList();
        }


        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax,
                                                       IParsedCSharpDocument document,
                                                       IFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {

            var arguments = GetImmutableMembers(syntax.SyntaxTree, context, location, out var classDeclarationSyntax);

            var constructor = ConstructorGenerator.GenerateSyntax(arguments, classDeclarationSyntax.Identifier.ValueText);

            var body = constructor.Body;

            foreach (var arg in arguments)
            {
                var memberName = arg.Item2;
                var parameterName = arg.Item2;

                if (ConstructorGenerator.ArgumentFirstLetterToLowerCase)
                {
                    parameterName = StringExtensions.FirstCharToLower(parameterName);
                }

                var expression = MemberAssignmentGenerator.GenerateSyntax(memberName, parameterName, true);

                body = body.AddStatements(SyntaxFactory.ExpressionStatement(expression));
            }

            constructor = constructor.ReplaceNode(constructor.Body, body);

            return new InsertSyntaxNodesWorkUnit()
            {
                HostNode = classDeclarationSyntax,
                SyntaxNodes = new List<SyntaxNode>() { constructor },
                Project = context.Project,
                Workspace = context.Project.Solution.Workspace,
            }.AsList();
        }

        List<Tuple<ITypeSymbol, string>> GetImmutableMembers(SyntaxTree syntaxTree,
                                                             IFeatureContext featureContext,
                                                             InteractionLocation location,
                                                             out ClassDeclarationSyntax classDeclaration)
        {
            classDeclaration = null;

            if (!featureContext.Project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            var root = syntaxTree.GetRoot();
            var model = compilation.GetSemanticModel(syntaxTree);

            if (model == null)
            {
                return null;
            }

            var token = root.FindToken(location.Position);

            if (!token.Span.Contains(location.Position))
            {
                return null;
            }

            classDeclaration = token.Parent as ClassDeclarationSyntax;
            if (classDeclaration == null)
            {
                return null;
            }

            var immutableFields = classDeclaration.Members.OfType<FieldDeclarationSyntax>()
                                                        .Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)))
                                                        .Where(f => !f.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)));

            var immutableProperties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                                                            .Where(p => p.AccessorList != null
                                                                    && !p.AccessorList.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.SetKeyword))
                                                                    && p.AccessorList.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.GetKeyword)));

            if (!immutableFields.Any() && !immutableProperties.Any())
            {
                return null;
            }

            var arguments = new List<Tuple<ITypeSymbol, string>>();

            foreach (var field in immutableFields)
            {
                var typeInfo = model.GetTypeInfo(field.Declaration.Type).Type;
                foreach (var v in field.Declaration.Variables)
                {
                    arguments.Add(new Tuple<ITypeSymbol, string>(typeInfo, v.Identifier.ValueText));
                }
            }

            foreach (var prop in immutableProperties)
            {
                var typeInfo = model.GetTypeInfo(prop.Type).Type;
                arguments.Add(new Tuple<ITypeSymbol, string>(typeInfo, prop.Identifier.ValueText));
            }

            return arguments;
        }
    }
}
