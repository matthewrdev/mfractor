using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Maui.Xmlns
{
    class XmlnsDefinitionCollection : IXmlnsDefinitionCollection
    {
        public XmlnsDefinitionCollection(IEnumerable<IXmlnsDefinition> xmlnsDefinitions)
        {
            XmlnsDefinitions = (xmlnsDefinitions ?? Enumerable.Empty<IXmlnsDefinition>()).ToList();
        }

        public IReadOnlyList<IXmlnsDefinition> XmlnsDefinitions { get;  }

        public IEnumerator<IXmlnsDefinition> GetEnumerator()
        {
            return XmlnsDefinitions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return XmlnsDefinitions.GetEnumerator();
        }

        public IXmlnsDefinition GetDefinitionForUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return default;
            }

            return XmlnsDefinitions.FirstOrDefault(xmlns => xmlns.Uri == uri);
        }
    }
}