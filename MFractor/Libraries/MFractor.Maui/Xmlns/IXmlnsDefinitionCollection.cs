using System;
using System.Collections;
using System.Collections.Generic;

namespace MFractor.Maui.Xmlns
{
    public interface IXmlnsDefinitionCollection : IEnumerable<IXmlnsDefinition>
    {
        public IReadOnlyList<IXmlnsDefinition> XmlnsDefinitions { get; }

        public IXmlnsDefinition GetDefinitionForUri(string uri);
    }
}