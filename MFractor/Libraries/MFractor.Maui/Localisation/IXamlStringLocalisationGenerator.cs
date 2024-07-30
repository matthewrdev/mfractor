using System.Collections.Generic;
using MFractor.Code.Documents;
using MFractor.Maui.Semantics;
using MFractor.Localisation;
using MFractor.Localisation.StringsProviders;
using MFractor.Xml;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Localisation
{
    interface IXamlLocalisableStringsProvider : ILocalisableStringsProvider
    {
        bool CanLocalise(XmlAttribute syntax, IXamlSemanticModel semanticModel, IXamlPlatform platform);

        IEnumerable<ILocalisableString> CollectTargetsForDocument(IParsedXmlDocument document, IXamlSemanticModel semanticModel, IXamlPlatform platform);

        IEnumerable<ILocalisableString> CollectTargetsForNode(XmlNode syntax, IParsedXmlDocument document, IXamlSemanticModel semanticModel, IXamlPlatform platform);
    }
}