
namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// An available code action that the user can invoke.
    /// </summary>
    public class CodeActionChoice : ICodeActionChoice
    {
        public ICodeActionSuggestion Suggestion { get; }

        public ICodeAction CodeAction { get; }

        public IFeatureContext Context { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeActionChoice"/> class.
        /// </summary>
        /// <param name="suggestion">Suggestion.</param>
        /// <param name="codeAction">Code action.</param>
        /// <param name="context">Context.</param>
        public CodeActionChoice(ICodeActionSuggestion suggestion,
                                ICodeAction codeAction,
                                IFeatureContext context)
        {
            Suggestion = suggestion ?? throw new System.ArgumentNullException(nameof(suggestion));
            CodeAction = codeAction ?? throw new System.ArgumentNullException(nameof(codeAction));
            Context = context ?? throw new System.ArgumentNullException(nameof(context));
        }
    }
}
