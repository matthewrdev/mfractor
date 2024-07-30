using MFractor.Code.CodeActions;


namespace MFractor.Maui.CodeActions
{
    /// <summary>
    /// A code action that performs a navigation operation.
    /// </summary>
    public abstract class NavigateXamlCodeAction : XamlCodeAction
    {
        /// <summary>
        /// The category; always <see cref="CodeActionCategory.Navigate"/>.
        /// </summary>
        /// <value>The category.</value>
        public override CodeActionCategory Category => CodeActionCategory.Navigate;
    }
}
