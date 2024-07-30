using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Localisation.Configuration;
using MFractor.Localisation.WorkUnits;
using MFractor.Utilities;
using MFractor.Utilities.SyntaxWalkers;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MFractor.Localisation.CodeActions
{
    class LocaliseStringLiteral : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Refactor;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.csharp.localise_string_literal";

        public override string Name => "Replace String With Resource Lookup";

        public override string Documentation => "Locates string literals in a C# file and moves them into RESX files.";

        [Import]
        public IDefaultResourceFile DefaultResourceFile
        {
            get; set;
        }

        public override bool CanExecute(SyntaxNode syntax,
                                        IParsedCSharpDocument document,
                                        IFeatureContext context,
                                        InteractionLocation location)
        {
            var literalExpression = StringSyntaxWalker.ResolveLiteralExpressionSyntax(syntax);

            if (literalExpression == null)
            {
                return false;
            }

            var kind = literalExpression.Token.Kind();

            if (kind != SyntaxKind.StringLiteralToken)
            {
                return false;
            }

            var projectFile = ProjectService.FindProjectFile(context.Project, filePath => Path.GetExtension(filePath) == ".resx");

            if (projectFile != null)
            {
                return true;
            }

            var projects = GetTargetProjects(context);

            return projects.Any();
        }

        IReadOnlyList<Project> GetTargetProjects(IFeatureContext context)
        {
            var projects = new List<Project>();

            foreach (var projectReference in context.Project.ProjectReferences)
            {
                var target = context.Solution.Projects.FirstOrDefault(p => p.Id == projectReference.ProjectId);

                if (target != null)
                {
                    var resxFile = ProjectService.FindProjectFile(target, filePath => Path.GetExtension(filePath) == ".resx");

                    if (resxFile != null)
                    {
                        projects.Add(target);
                    }
                }
            }

            return projects;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax,
                                                                  IParsedCSharpDocument document,
                                                                  IFeatureContext context,
                                                                  InteractionLocation location)
        {
            return CreateSuggestion("Replace with resx resource lookup", 0).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax,
                                                       IParsedCSharpDocument document,
                                                       IFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            var literalExpression = StringSyntaxWalker.ResolveLiteralExpressionSyntax(syntax);

            var projects = GetTargetProjects(context);

            return new LocaliseDocumentWorkUnit(context.Document,
                                                      context.Project,
                                                      projects,
                                                      context.SemanticModel,
                                                      literalExpression.Span,
                                                      DefaultResourceFile.ProjectFilePath).AsList();
        }
    }
}
