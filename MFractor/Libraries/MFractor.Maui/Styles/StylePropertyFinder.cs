using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Maui.Semantics;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Styles
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IStylePropertyFinder))]
    class StylePropertyFinder : IStylePropertyFinder
    {
        public IReadOnlyDictionary<string, string> FindCandidateStyleProperties(XmlNode node, IXamlSemanticModel xamlSemanticModel, IXamlPlatform platform)
        {
            var targetTypeSymbol = xamlSemanticModel.GetSymbol(node) as INamedTypeSymbol;

            var attrs = node.GetAttributes(a =>
            {
                if (!(SymbolHelper.FindMemberSymbolByName(targetTypeSymbol, a.Name.LocalName) is IPropertySymbol property))
                {
                    return false;
                }

                if (property.Name == "Style" && SymbolHelper.DerivesFrom(property.Type, platform.Style.MetaType))
                {
                    return false;
                }

                return true;
            });

            var properties = new Dictionary<string, string>();

            foreach (var attr in attrs)
            {
                if (attr.Name.FullName == "Text"
                    || attr.Name.FullName == "Placeholder")
                {
                    continue;
                }

                properties[attr.Name.FullName] = attr.Value.Value;
            }

            return properties;
        }
    }
}