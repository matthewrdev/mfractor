using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Code.Documents;
using MFractor.Code.Formatting;
using MFractor.Configuration;
using MFractor.IOC;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// A code action that can execute in a given document and context.
    /// </summary>
    [Export(typeof(ICodeAction))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public abstract class CodeAction : Configurable, ICodeAction
    {
        readonly Lazy<IProjectService> projectService = new Lazy<IProjectService>(() => Resolver.Resolve<IProjectService>());

        /// <summary>
        /// Gets the project service.
        /// </summary>
        /// <value>The project service.</value>
        protected IProjectService ProjectService => projectService.Value;

        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService = new Lazy<ICodeFormattingPolicyService>(() => Resolver.Resolve<ICodeFormattingPolicyService>());

        /// <summary>
        /// Gets the formatting policy service.
        /// </summary>
        /// <value>The formatting policy service.</value>
        protected ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        /// <summary>
        /// The category that this code action belongs to.
        /// </summary>
        /// <returns>The category.</returns>
        public abstract CodeActionCategory Category { get; }

        /// <summary>
        /// The filter that this code action supports execution.
        /// </summary>
        /// <value>The scope to target.</value>
        public abstract DocumentExecutionFilter Filter { get; }

        /// <summary>
        /// The analytics event of this code action.
        /// </summary>
        /// <value>The analytics event.</value>
        public virtual string AnalyticsEvent => Name;

        /// <summary>
        /// Should this code action always surface within MFractors tooltips as a common code action that users might perform?
        /// <para/>
        /// For example, when a color or string literal is detect in code, setting this as a priority action nudges users to localise the string or move the color to a resource.
        /// <para/>
        /// Priority code actions encourage developers to use best practices, teach them about new methodologies
        /// </summary>
        public virtual bool IsPriorityAction => false;

        /// <summary>
        /// A factory method to create a new code action suggestion.
        /// </summary>
        /// <returns>The suggestion.</returns>
        /// <param name="description">Description.</param>
        public ICodeActionSuggestion CreateSuggestion(string description)
        {
            return CreateSuggestion(description, 0);
        }

        /// <summary>
        /// A factory method to create a new code action suggestion.
        /// </summary>
        /// <returns>The suggestion.</returns>
        /// <param name="description">Description.</param>
        /// <param name="id">Identifier.</param>
		public ICodeActionSuggestion CreateSuggestion(string description, int id)
        {
            return new CodeActionSuggestion(description, Identifier, id);
        }

        /// <summary>
        /// A factory method to create a new code action suggestion.
        /// </summary>
        /// <returns>The suggestion.</returns>
        /// <param name="description">Description.</param>
        /// <param name="id">Identifier.</param>
        public ICodeActionSuggestion CreateSuggestion(string description, Enum id)
        {
            return new CodeActionSuggestion(description, Identifier, Convert.ToInt32(id));
        }

        /// <summary>
        /// Can this code action operation execute in the given context.
        /// </summary>
        /// <returns><c>true</c>, if the refactoring can execute, <c>false</c> otherwise.</returns>
        /// <param name="context">The code action bundle</param>
        /// <param name="location">The execution context in the documentation that triggered the action</param>
        public abstract bool CanExecute(IFeatureContext context, InteractionLocation location);

        /// <summary>
        /// Execute the code action, building a collection of workUnits that should be applied by the IDE.
        /// </summary>
        /// <param name="context">Bundle.</param>
        /// <param name="suggestion">The code action suggestion to execute</param>
        /// <param name="location">The execution context in the documentation that triggered the action</param>
        public abstract IReadOnlyList<IWorkUnit> Execute(IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location);

        /// <summary>
        /// If the code action can execute against a specific document.
        /// <para/>
        /// Use this to disable the code action based on a cheap evaulation condition.
        /// <para/>
        /// For instance, if a certain namespace is expected to be present, returning false can skip unnecessary validation the refactoring
        /// would otherwise have to do in the CanExecute method.
        /// </summary>
        /// <returns><c>true</c>, if this refactoring operation can execute against the document, <c>false</c> otherwise.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">The context for the code action</param>
        public abstract bool IsInterestedInDocument(IParsedDocument document, IFeatureContext context);

        /// <summary>
        /// Suggest a list of available code actions within the context.
        /// </summary>
        /// <returns>The actions that this operation supports</returns>
        /// <param name="context">Bundle.</param>
        /// <param name="location">The location (position and selection) the code action was triggered at</param>
        public abstract IReadOnlyList<ICodeActionSuggestion> Suggest(IFeatureContext context, InteractionLocation location);

        /// <summary>
        /// Tries to get the <typeparamref name="TPreprocessor"/> from the current <paramref name="context"/>.
        /// </summary>
        /// <typeparam name="TPreprocessor"></typeparam>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected bool TryGetPreprocessor<TPreprocessor>(IFeatureContext context, out TPreprocessor result) where TPreprocessor : ICodeAnalysisPreprocessor
        {
            result = default;
            var key = typeof(TPreprocessor);

            if (context.MetaData.TryGetValue(key.FullName, out var value)
                && value is TPreprocessor preprocessor)
            {
                result = preprocessor;
                return preprocessor.IsValid;
            }

            return false;
        }
    }
}
