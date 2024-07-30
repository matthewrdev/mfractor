namespace MFractor.Maui.Styles
{
    /// <summary>
    /// A <see cref="IStylePropertyValue"/> that contains a literal value.
    /// </summary>
    public interface ILiteralStylePropertyValue : IStylePropertyValue
    {
        /// <summary>
        /// The value of this style property setter.
        /// </summary>
        string Value { get; }
    }
}