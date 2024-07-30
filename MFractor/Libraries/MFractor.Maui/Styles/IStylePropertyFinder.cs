using System;
using System.Collections.Generic;
using MFractor.Maui.Semantics;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Styles
{
    public interface IStylePropertyFinder
    {
        IReadOnlyDictionary<string, string> FindCandidateStyleProperties(Xml.XmlNode node, IXamlSemanticModel xamlSemanticModel, IXamlPlatform platform);
    }
}