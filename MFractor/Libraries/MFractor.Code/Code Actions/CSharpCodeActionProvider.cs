using System;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Configuration;
using MFractor.Code.Documents;
using MFractor.Licensing;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using MFractor.Workspace;

namespace MFractor.Code.CodeActions
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "MFractor C# Refactorings"), Shared]
    public sealed class CSharpCodeActionProvider : CodeRefactoringProvider
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        Lazy<IProjectService> ProjectServiceContainer { get; set; }

        [Import]
        Lazy<ICodeActionEngine> CodeActionEngineContainer { get; set; }

        [Import]
        Lazy<ILicensingService> LicensingServiceContainer { get; set; }

        [Import]
        Lazy<IAnalyticsService> AnalyticsServiceContainer { get; set; }

        [Import]
        Lazy<IWorkEngine> WorkEngineContainer { get; set; }

        ICodeActionEngine CodeActionEngine => CodeActionEngineContainer.Value;

        IProjectService ProjectService => ProjectServiceContainer.Value;

        ILicensingService LicensingService => LicensingServiceContainer.Value;

        IAnalyticsService AnalyticsService => AnalyticsServiceContainer.Value;

        IWorkEngine WorkEngine => WorkEngineContainer.Value;

        static readonly CodeActionCategory[] categoryFilters = {
            CodeActionCategory.Fix,
            CodeActionCategory.Refactor,
            CodeActionCategory.Generate,
            CodeActionCategory.Organise,
            CodeActionCategory.Misc,
            CodeActionCategory.Find,
        };

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!LicensingService.IsPaid)
            {
                return;
            }

            try
            {
                var guid = ProjectService.GetProjectGuid(context.Document.Project);
                var name = context.Document.Project.Name;

                if (string.IsNullOrEmpty(guid))
                {
                    return;
                }

                var configuration = ConfigurationId.Create(guid, name);

                var syntaxTree = await context.Document.GetSyntaxTreeAsync();

                var syntax = syntaxTree.GetRoot().FindToken(context.Span.Start).Parent;

                if (string.IsNullOrEmpty(context.Document.FilePath) || syntaxTree == null)
                {
                    return;
                }


                context.Document.TryGetSemanticModel(out var semanticModel);



                var filePath = context.Document.FilePath;
                var projectFile = ProjectService.GetProjectFileWithFilePath(context.Document.Project, filePath);

                var featureContext = new FeatureContext(context.Document.Project.Solution.Workspace,
                                                        context.Document.Project.Solution,
                                                        context.Document.Project,
                                                        new ParsedCSharpDocument(filePath, syntaxTree, projectFile),
                                                        syntax,
                                                        semanticModel,
                                                        configuration);

                var location = new InteractionLocation(context.Span.Start, context.Span);

                var codeActions = CodeActionEngine.RetrieveCodeActions(featureContext, location, categoryFilters);

                if (codeActions != null && codeActions.Any())
                {
                    foreach (var ca in codeActions)
                    {
                        var suggestions = ca.Suggest(featureContext, location);
                        foreach (var suggestion in suggestions)
                        {
                            context.RegisterRefactoring(new WorkEngineCodeAction(featureContext, ca, location, suggestion, AnalyticsService, WorkEngine));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
