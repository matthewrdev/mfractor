using MFractor.Code.CodeActions;

namespace MFractor.Maui.CodeActions
{
    /// <summary>
    /// A code action that refactors XAML.
    /// </summary>
    public abstract class RefactorXamlCodeAction : XamlCodeAction
    {
        /// <summary>
        /// The category; always <see cref="CodeActionCategory.Refactor"/>.
        /// </summary>
        /// <returns>The category.</returns>
        public sealed override CodeActionCategory Category => CodeActionCategory.Refactor;
    }
}
