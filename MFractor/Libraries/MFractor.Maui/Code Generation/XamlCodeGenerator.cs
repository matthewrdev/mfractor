using MFractor.Code.CodeGeneration;

namespace MFractor.Maui.CodeGeneration
{
    /// <summary>
    /// A code generator that creates only XAML code.
    /// </summary>
    public abstract class XamlCodeGenerator : CodeGenerator
    {
        /// <summary>
        /// Gets the languages that this code generator supports.
        /// </summary>
        /// <value>The languages.</value>
        public sealed override string[] Languages { get; } = new string[] { "XAML" };
    }
}
