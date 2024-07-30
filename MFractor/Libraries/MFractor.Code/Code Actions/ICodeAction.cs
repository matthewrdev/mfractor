using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Configuration;
using MFractor.Code.Documents;
using MFractor.Work;
using MFractor.Analytics;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// A code action that can execute in a given document and context.
    /// </summary>
    [InheritedExport]
    public interface ICodeAction : IConfigurable, IAnalyticsFeature
    {
        /// <summary>
        /// The category that this code action belongs to.
        /// </summary>
        /// <returns>The category.</returns>
        CodeActionCategory Category { get; }

        /// <summary>
        /// Shoudl this code action always surface within MFractors tooltips as a common code action that users might perform?
        /// <para/>
        /// For example, when a color or string literal is detect in code, setting this as a priority action nudges users to localise the string or move the color to a resource.
        /// <para/>
        /// Priority code actions encourage developers to use best practices, teach them about new methodologies
        /// </summary>
        bool IsPriorityAction { get; }

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
        bool IsInterestedInDocument (IParsedDocument document, IFeatureContext context);

        /// <summary>
        /// The filter that this code action supports execution.
        /// </summary>
        /// <value>The scope to target.</value>
        DocumentExecutionFilter Filter { get; }

        /// <summary>
        /// Can this code action operation execute in the given context.
        /// </summary>
        /// <returns><c>true</c>, if the refactoring can execute, <c>false</c> otherwise.</returns>
        /// <param name="context">The code action bundle</param>
        /// <param name="location">The execution context in the documentation that triggered the action</param>
        bool CanExecute(IFeatureContext context, InteractionLocation location);

        /// <summary>
        /// Suggest a list of available code actions within the context. 
        /// </summary>
        /// <returns>The actions that this operation supports</returns>
        /// <param name="context">Bundle.</param>
        /// <param name="location">The location (position and selection) the code action was triggered at</param>
        IReadOnlyList<ICodeActionSuggestion> Suggest (IFeatureContext context, InteractionLocation location);

        /// <summary>
        /// Execute the code action, building a collection of work units that should be applied by the IDE.
        /// </summary>
        /// <param name="context">Bundle.</param>
        /// <param name="suggestion">The code action suggestion to execute</param>
        /// <param name="location">The execution context in the documentation that triggered the action</param>
        IReadOnlyList<IWorkUnit> Execute (IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location);
    }
}
