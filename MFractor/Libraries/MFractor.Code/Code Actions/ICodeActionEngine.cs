using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Progress;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// The <see cref="ICodeActionEngine"/> is the central API for getting code actions that can execute against certain <see cref="Documents.IParsedDocument"/>'s, <see cref="FeatureContext"/>'s and <see cref="InteractionLocation"/>'s.
    /// </summary>
    public interface ICodeActionEngine
    {
        /// <summary>
        /// Executes the provided code action choice.
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="interactionLocation"></param>
        /// <returns></returns>
        Task<bool> Execute(ICodeActionChoice choice, InteractionLocation interactionLocation, IProgressMonitor progressMonitor = null);

        /// <summary>
        /// Gets the code actions for the provided <paramref name="category"/>.
        /// </summary>
        /// <returns>The code actions for category.</returns>
        /// <param name="category">Category.</param>
        IEnumerable<ICodeAction> GetCodeActionsForCategory(CodeActionCategory category);

        /// <summary>
        /// Gets the code actions that filter.
        /// </summary>
        IEnumerable<ICodeAction> GetCodeActionsForFilter(DocumentExecutionFilter filter);

        /// <summary>
        /// Retrieves the code actions that can execute within the provided <paramref name="context"/> at the <paramref name="location"/>.
        /// </summary>
        IEnumerable<ICodeAction> RetrieveCodeActions(IFeatureContext context,
                                                     InteractionLocation location);

        /// <summary>
        /// Retrieves the code actions that can execute within the provided <paramref name="context"/> at the <paramref name="location"/>, filtered by the given <paramref name="predicate"/>.
        /// </summary>
        IEnumerable<ICodeAction> RetrieveCodeActions(IFeatureContext context,
                                                     InteractionLocation location,
                                                     Func<ICodeAction, bool> predicate);

        /// <summary>
        /// Retrieves the code actions that can execute within the provided <paramref name="context"/> at the <paramref name="location"/> of <paramref name="category"/>.
        /// </summary>
        IEnumerable<ICodeAction> RetrieveCodeActions(IFeatureContext context,
                                                     InteractionLocation location,
                                                     CodeActionCategory category);

        /// <summary>
        /// Retrieves the code actions that can execute within the provided <paramref name="context"/> at the <paramref name="location"/> of <paramref name="categories"/>.
        /// </summary>
        IEnumerable<ICodeAction> RetrieveCodeActions(IFeatureContext context,
                                                     InteractionLocation location,
                                                     IEnumerable<CodeActionCategory> categories);

        /// <summary>
        /// Retrieves the code actions marked with the <see cref="CommonCodeActionAttribute"/> that can execute within the provided <paramref name="context"/> at the <paramref name="location"/>.
        /// </summary>
        IEnumerable<ICodeAction> RetrieveCommonCodeActions(IFeatureContext context,
                                                           InteractionLocation location);
    }
}
