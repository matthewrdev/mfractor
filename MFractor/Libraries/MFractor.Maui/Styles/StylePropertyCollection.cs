using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Maui.Styles
{
    class StylePropertyCollection : IStylePropertyCollection
    {
        public StylePropertyCollection(Dictionary<string, List<IStyleProperty>> properties)
        {
            AllProperties = properties.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<IStyleProperty>)pair.Value);
            Properties = properties.Values.Select(v => v.OrderBy(p => p.Priority).First()).ToList();
        }

        public IReadOnlyDictionary<string, IReadOnlyList<IStyleProperty>> AllProperties { get; }

        public IReadOnlyList<IStyleProperty> Properties { get; }

        public int Count => Properties.Count;

        public IEnumerator<IStyleProperty> GetEnumerator()
        {
            return Properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Properties.GetEnumerator();
        }
    }
}