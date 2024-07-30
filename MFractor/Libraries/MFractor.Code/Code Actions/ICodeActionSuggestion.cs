using System;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// Code action suggestion.
    /// </summary>
    public interface ICodeActionSuggestion
    {
        /// <summary>
        /// The <see cref="ICodeAction.Identifier"/> of the code action that owns this suggestion.
        /// </summary>
        string CodeActionIdentifier { get; }

        /// <summary>
        /// A description of what the code action will do when executed.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; }

        /// <summary>
        /// When an <see cref="ICodeAction"/> returns multiple suggestions, a unique id of the action to execute.
        /// </summary>
        /// <value>The action identifier.</value>
        int ActionId { get; }

        /// <summary>
        /// Gets the <see cref="ActionId"/> cast as <typeparamref name="TEnum"/>.
        /// </summary>
        TEnum GetAction<TEnum>() where TEnum : Enum;

        /// <summary>
        /// Is the code action suggestion the provided <paramref name="action"/>?
        /// </summary>
        bool IsAction(Enum action);
    }
}