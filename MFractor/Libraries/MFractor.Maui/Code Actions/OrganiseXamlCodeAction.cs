using MFractor.Code.CodeActions;


namespace MFractor.Maui.CodeActions
{
    /// <summary>
    /// A code action that organises XAML.
    /// </summary>
    public abstract class OrganiseXamlCodeAction : XamlCodeAction
	{
        /// <summary>
        /// The category; always <see cref="CodeActionCategory.Organise"/>.
        /// </summary>
        /// <returns>The category.</returns>
        public sealed override CodeActionCategory Category => CodeActionCategory.Organise;
    }
}
