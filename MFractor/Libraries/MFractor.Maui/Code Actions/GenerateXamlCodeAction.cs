using MFractor.Code.CodeActions;


namespace MFractor.Maui.CodeActions
{
    /// <summary>
    /// A code action for XAML that generates code.
    /// <para/>
    /// Code actions that implement this class will appear within the Generate menu.
    /// </summary>
    public abstract class GenerateXamlCodeAction : XamlCodeAction
    {
        /// <summary>
        /// The category; always <see cref="CodeActionCategory.Generate"/>.
        /// </summary>
        /// <returns>The category.</returns>
        public sealed override CodeActionCategory Category => CodeActionCategory.Generate;
    }
}
