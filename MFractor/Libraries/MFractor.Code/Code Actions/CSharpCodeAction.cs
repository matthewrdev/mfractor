using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Documents;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// A code action that targets C# code files and syntax trees.
    /// </summary>
    public abstract class CSharpCodeAction : CodeAction
    {
        /// <summary>
        /// Can this code action operation execute in the given context?
        public sealed override bool CanExecute(IFeatureContext context, InteractionLocation location)
        {
            var parsedDocument = context.Document as ParsedCSharpDocument;
            if (parsedDocument == null)
            {
                return false;
            }

            ApplyConfiguration(context.ConfigurationId);

            if (Filter == CSharpCodeActionExecutionFilters.CSharpDocument)
            {
                return CanExecute(parsedDocument, context, location);
            }

            var syntax = context.GetSyntax(default(SyntaxNode));
            if (syntax == null)
            {
                return false;
            }

            return CanExecute(syntax, parsedDocument, context, location);
        }

        /// <summary>
        /// Suggest a list of available code actions within the context.
        /// </summary>
        /// <returns>The actions that this operation supports</returns>
        /// <param name="context">Bundle.</param>
        /// <param name="location">The location (position and selection) the code action was triggered at</param>
        public sealed override IReadOnlyList<ICodeActionSuggestion> Suggest(IFeatureContext context, InteractionLocation location)
        {
            var parsedDocument = context.Document as ParsedCSharpDocument;
            if (parsedDocument == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            ApplyConfiguration(context.ConfigurationId);

            if (this.Filter == CSharpCodeActionExecutionFilters.CSharpDocument)
            {
                return Suggest(parsedDocument, context, location);
            }

            var syntax = context.GetSyntax(default(SyntaxNode));
            if (syntax == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            return Suggest(syntax, parsedDocument, context, location);
        }

        /// <summary>
        /// Execute the code action, building a collection of workUnits that should be applied by the IDE.
        /// </summary>
        /// <param name="context">Bundle.</param>
        /// <param name="suggestion">The code action suggestion to execute</param>
        /// <param name="location">The execution context in the documentation that triggered the action</param>
        public sealed override IReadOnlyList<IWorkUnit> Execute(IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var parsedDocument = context.Document as IParsedCSharpDocument;
            if (parsedDocument == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            ApplyConfiguration(context.ConfigurationId);
            if (this.Filter == CSharpCodeActionExecutionFilters.CSharpDocument)
            {
                return Execute(parsedDocument, context, suggestion, location);
            }

            var syntax = context.GetSyntax(default(SyntaxNode));
            if (syntax == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            return Execute(syntax, parsedDocument, context, suggestion, location);
        }

        /// <summary>
        /// Can the code action execute in the current <paramref name="document"/>?
        /// </summary>
        public virtual bool CanExecute(ParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return false;
        }

        /// <summary>
        /// Can the code action execute against the current <paramref name="syntax"/>, <paramref name="document"/> and <paramref name="context"/>?
        /// </summary>
        public virtual bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return false;
        }

        /// <summary>
        /// Suggest available code action choices to execute in the given document.
        /// </summary>
        public virtual IReadOnlyList<ICodeActionSuggestion> Suggest(IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return Array.Empty<ICodeActionSuggestion>();
        }

        /// <summary>
        /// Suggest available code action choices to execute in the given document.
        /// </summary>
        public virtual IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return Array.Empty<ICodeActionSuggestion>();
        }

        /// <summary>
        /// Execute the code action using the given <paramref name="context"/>, <paramref name="context"/> and <paramref name="suggestion"/>.
        /// </summary>
        public virtual IReadOnlyList<IWorkUnit> Execute(IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return Array.Empty<IWorkUnit>();
        }

        /// <summary>
        /// Execute the code action using the given <paramref name="syntax"/>, <paramref name="context"/>, <paramref name="context"/> and <paramref name="suggestion"/>.
        /// </summary>
        public virtual IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return Array.Empty<IWorkUnit>();
        }

        /// <summary>
        /// If the code action can execute against a specific document.
        /// <para/>
        /// Use this to disable the code action based on a cheap evaulation condition.
        /// <para/>
        /// For instance, if a certain namespace is expected to be present, returning false can skip unnecessary validation the refactoring
        /// would otherwise have to do in the CanExecute method.
        /// </summary>
        public sealed override bool IsInterestedInDocument(IParsedDocument document, IFeatureContext context)
        {
            var parsedDocument = document as IParsedCSharpDocument;
            if (parsedDocument == null)
            {
                return false;
            }

            return IsAvailableInDocument(parsedDocument, context);
        }

        /// <summary>
        /// Is this code action available for execution in the given <paramref name="context"/> and <paramref name="document"/>?
        /// </summary>
        protected virtual bool IsAvailableInDocument(IParsedCSharpDocument document, IFeatureContext context)
        {
            return true;
        }

        public IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode element, IFeatureContext context, InteractionLocation location)
        {
            var parsedDocument = context.Document as IParsedCSharpDocument;
            if (parsedDocument == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            if (Filter == CSharpCodeActionExecutionFilters.CSharpDocument)
            {
                return Suggest(parsedDocument, context, location);
            }

            if (Filter == CSharpCodeActionExecutionFilters.SyntaxNode)
            {
                return Suggest(element, parsedDocument, context, location);
            }

            return Array.Empty<ICodeActionSuggestion>();
        }

        public IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var parsedDocument = context.Document as IParsedCSharpDocument;
            if (parsedDocument == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            return Execute(syntax, parsedDocument, context, suggestion, location);
        }
    }
}
