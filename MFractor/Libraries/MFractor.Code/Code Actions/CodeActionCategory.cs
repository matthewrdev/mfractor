using System;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// A category that is used to group an <see cref="ICodeAction"/>.
    /// <para/>
    /// In the outer application, code actions will be grouped within context menus by these categories.
    /// </summary>
    public enum CodeActionCategory
    {
        /// <summary>
        /// Code actions that fix a code issue detected by an analyser.
        /// </summary>
        Fix,

        /// <summary>
        /// Code actions that refactor code.
        /// </summary>
        Refactor,

        /// <summary>
        /// Code actions that re-shuffle and organize code.
        /// </summary>
        Organise,

        /// <summary>
        /// Code actions that generate code.
        /// </summary>
        Generate,

        /// <summary>
        /// Code actions that navigates to another file or part of the code.
        /// </summary>
        Navigate,

        /// <summary>
        /// Code actions that find related items.
        /// </summary>
        Find,

        /// <summary>
        /// An unclassified code action.
        /// </summary>
        Misc,
    }
}
