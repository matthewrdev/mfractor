namespace MFractor.Maui.XamlPlatforms
{
    public interface ITypeDefinition
    {
        /// <summary>
        /// The fully qualified type name (namespace and class/enum/interface/struct name).
        /// </summary>
        string MetaType { get; }

        /// <summary>
        /// The full namespace path of the <see cref="MetaType"/>.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// The name of the type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The name of the type, excluding the generic arguments if any.
        /// </summary>
        string NonGenericName { get; }

        /// <summary>
        /// The name of this type when used inside a markup expression
        /// </summary>
        string MarkupExpressionName { get; }


        bool IsGeneric { get; }
    }
}
