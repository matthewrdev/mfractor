using System.Collections.Generic;

namespace MFractor.Maui.Styles
{
    /// <summary>
    /// A collection of <see cref="IStyleProperty"/> 
    /// </summary>
    public interface IStylePropertyCollection : IEnumerable<IStyleProperty>
    {
        IReadOnlyDictionary<string, IReadOnlyList<IStyleProperty>> AllProperties { get; }

        IReadOnlyList<IStyleProperty> Properties { get; }

        int Count { get; }
    }
}
