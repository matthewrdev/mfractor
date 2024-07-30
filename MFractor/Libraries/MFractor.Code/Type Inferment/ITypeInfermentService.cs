using Microsoft.CodeAnalysis;

namespace MFractor.Code.TypeInferment
{
    /// <summary>
    /// The <see cref="ITypeInfermentService"/> can be used to infer the meta type for a name/value pair.
    /// </summary>
    public interface ITypeInfermentService
    {
        /// <summary>
        /// Infer the type from the provided <paramref name="name"/> and <paramref name="value"/>.
        /// </summary>
        /// <returns>The type from name and value.</returns>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        /// <param name="colorType">Color type.</param>
        /// <param name="imageType">Image type.</param>
        /// <param name="defaultType">Default type.</param>
        string InferTypeFromNameAndValue(string name, string value, string colorType, string imageType, string defaultType, Compilation compilation);
    }
}
