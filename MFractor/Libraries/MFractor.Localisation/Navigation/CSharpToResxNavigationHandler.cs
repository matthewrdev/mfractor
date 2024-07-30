using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Ide.WorkUnits;
using MFractor.Navigation;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation.Navigation
{
    class CSharpToResxNavigationHandler : NavigationHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<ILocalisationResolver> localisationResolver;
        public ILocalisationResolver LocalisationResolver => localisationResolver.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public CSharpToResxNavigationHandler(Lazy<ILocalisationResolver> localisationResolver,
                                             Lazy<IProjectService> projectService)
        {
            this.localisationResolver = localisationResolver;
            this.projectService = projectService;
        }

        public override bool IsAvailable(INavigationContext navigationContext)
        {
            return Path.GetExtension(navigationContext.FilePath) == ".cs";
        }

        public override Task<IReadOnlyList<IWorkUnit>> Navigate(INavigationContext navigationContext, INavigationSuggestion navigationSuggestion)
        {
            var localisations = GetLocalisations(navigationContext);

            var navigations = new List<NavigateToFileSpanWorkUnit>();

            foreach (var localisation in localisations)
            {
                navigations.Add(new NavigateToFileSpanWorkUnit(localisation.KeySpan, localisation.ProjectFile.FilePath));
            }

            return Task.FromResult<IReadOnlyList<IWorkUnit>>(new NavigateToFileSpansWorkUnit(navigations).AsList());
        }

        public override Task<INavigationSuggestion> Suggest(INavigationContext navigationContext)
        {
            var localisations = GetLocalisations(navigationContext);
            if (localisations is null)
            {
                return Task.FromResult<INavigationSuggestion>(default);
            }

            return Task.FromResult(CreateSuggestion("Navigate To Localisation"));
        }

        ILocalisationDeclarationCollection GetLocalisations(INavigationContext navigationContext)
        {
            if (!navigationContext.CompilationProject.TryGetCompilation(out var compilation))
            {
                return default;
            }

            var document = navigationContext.CompilationProject.Documents.FirstOrDefault(d => d.FilePath == navigationContext.FilePath);

            if (document is null)
            {
                return default;
            }

            if (!document.TryGetSyntaxTree(out var syntaxTree))
            {
                return default;
            }

            var syntax = GetSyntaxAtLocation(syntaxTree, navigationContext.CaretOffset);

            if (syntax is null)
            {
                return default;
            }

            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var symbolInfo = semanticModel.GetSymbolInfo(syntax);
            var property = symbolInfo.Symbol as IPropertySymbol;
            if (property is null)
            {
                return default;
            }

            return LocalisationResolver.ResolveLocalisations(navigationContext.CompilationProject, property);
        }

        public ExpressionSyntax GetSyntaxAtLocation(SyntaxTree syntaxTree, int offset)
        {
            var syntaxRoot = syntaxTree.GetRoot();

            var span = TextSpan.FromBounds(offset, offset + 1);

            SyntaxNode node = null;
            try
            {
                if (syntaxRoot.Span.IntersectsWith(span) && span.Start >= syntaxRoot.SpanStart && span.End <= syntaxRoot.Span.End)
                {
                    node = syntaxRoot.FindNode(span);
                }
            }
            catch (ArgumentOutOfRangeException aex)
            {
                log?.Info(aex.ToString());
            }
            catch (Exception ex)
            {
                log?.Warning(ex.ToString());
            }

            if (node is ExpressionSyntax expressionSyntax)
            {
                return expressionSyntax;
            }

            return default;
        }
    }
}