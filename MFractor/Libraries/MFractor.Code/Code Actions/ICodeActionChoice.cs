namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// An available code action that the user can invoke.
    /// </summary>
    public interface ICodeActionChoice
    {
        /// <summary>
        /// The code action suggestion; a description to display within the user interface and an integer id.
        /// </summary>
        ICodeActionSuggestion Suggestion { get; }

        /// <summary>
        /// The <see cref="ICodeAction"/> to execute if this choice is selected.
        /// </summary>
        ICodeAction CodeAction { get; }

        /// <summary>
        /// The relevant <see cref="IFeatureContext"/> for this choice.
        /// </summary>
        IFeatureContext Context { get; }
    }
}