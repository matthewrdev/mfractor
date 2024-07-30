using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeGeneration;


namespace MFractor.Xml.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ISortedAttributeGenerator))]
    class SortedAttributeGenerator : CodeGenerator, ISortedAttributeGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.xaml.xml.sorted_attributes";

        public override string Documentation => "Given a collection of XML attributes this code generator sorts them by namespace and name.";

        public override string Name => "Sort Attributes";

        public override string[] Languages { get; } = new string[] { "XML" };

        public IEnumerable<XmlAttribute> Generate(IReadOnlyList<XmlAttribute> attributes)
        {
            if (attributes is null)
            {
                return Enumerable.Empty<XmlAttribute>();
            }

            var originalAttributes = new List<XmlAttribute>(attributes);

            var sortedAttributes = new List<XmlAttribute>();

            var namespaceAttributes = originalAttributes.Where(attr => attr.Name.FullName == "xmlns" || (attr.Name.HasNamespace && attr.Name.Namespace == "xmlns")).OrderBy(attr => attr.Name.FullName);
            if (namespaceAttributes.Any())
            {
                sortedAttributes.AddRange(namespaceAttributes);
                originalAttributes.RemoveAll(attr => namespaceAttributes.Contains(attr));
            }

            var implicitNamespacedAttrs = originalAttributes.Where(attr => attr.Name.HasNamespace == false).ToList();
            if (implicitNamespacedAttrs.Any())
            {
                implicitNamespacedAttrs = implicitNamespacedAttrs.OrderBy(attr => attr.Name.FullName).ToList();
                sortedAttributes.AddRange(implicitNamespacedAttrs);
                originalAttributes.RemoveAll(attr => implicitNamespacedAttrs.Contains(attr));
            }

            var explicitNamespacedAttrs = originalAttributes.Where(attr => attr.Name.HasNamespace).ToList();
            if (explicitNamespacedAttrs.Any())
            {
                explicitNamespacedAttrs = explicitNamespacedAttrs.OrderBy(attr => attr.Name.FullName).ToList();
                sortedAttributes.AddRange(explicitNamespacedAttrs);
                originalAttributes.RemoveAll(attr => explicitNamespacedAttrs.Contains(attr));
            }

            return sortedAttributes;
        }

        public IEnumerable<XmlAttribute> Generate(XmlNode node)
        {
            if (!node.HasAttributes)
            {
                return Enumerable.Empty<XmlAttribute>();
            }

            return Generate(node.Attributes);
        }
    }
}
