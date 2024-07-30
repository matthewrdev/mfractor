using System.Collections.Generic;
using System.Drawing;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.Colors
{
    public class DuplicatedColorValuesInDocumentBundle
    {
        public DuplicatedColorValuesInDocumentBundle(IReadOnlyList<XmlAttribute> attributes,
                                                     Color color,
                                                     string staticResourceName = "")
        {
            Attributes = attributes ?? throw new System.ArgumentNullException(nameof(attributes));
            Color = color;
            StaticResourceName = staticResourceName;
        }

        public IReadOnlyList<XmlAttribute> Attributes { get; }

        public Color Color { get; }

        public string StaticResourceName { get; }
    }
}
