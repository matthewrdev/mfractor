using System;
namespace MFractor.Views
{
    /// <summary>
    /// A factory to create an <see cref="ITextEditor"/>
    /// </summary>
    public interface ITextEditorFactory
    {
        /// <summary>
        /// Create a new <see cref="ITextEditor"/>.
        /// </summary>
        /// <returns></returns>
        ITextEditor Create();
    }
}