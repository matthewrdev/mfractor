using System;

namespace MFractor.Configuration.PropertyConverters
{
    /// <summary>
    /// A strongly typed configuration property converter.
    /// </summary>
    public abstract class TypedPropertyConverter<TType> : PropertyConverter
    {
        public sealed override bool Supports(Type type)
        {
            return type == typeof(TType);
        }
    }
}
