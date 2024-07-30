using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.Documents;
using MFractor.Maui.Semantics;
using MFractor.Localisation;
using MFractor.Localisation.StringsProviders;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Localisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXamlLocalisableStringsProvider))]
    class XamlLocalisableStringsProvider : IXamlLocalisableStringsProvider, ILocalisableStringsProvider
    {
        public string[] SupportedFileExtensions { get; } = new string[] { ".xaml" };

        [Import]
        public IXamlPlatformRepository XamlPlatforms { get; set; }

        public IEnumerable<ILocalisableString> RetrieveLocalisableStrings(IParsedDocument document, object semanticModel)
        {
            if (!(document is ParsedXamlDocument xamlDocument)
                || !(semanticModel is IXamlSemanticModel))
            {
                return Enumerable.Empty<ILocalisableString>();
            }

            var platform = XamlPlatforms.ResolvePlatform(document.ProjectFile.CompilationProject);
            if (platform is null)
            {
                return Enumerable.Empty<ILocalisableString>();
            }

            return CollectTargetsForDocument(xamlDocument, semanticModel as IXamlSemanticModel, platform).OrderBy(t => t.Span.Start);
        }

        public IEnumerable<ILocalisableString> CollectTargetsForDocument(IParsedXmlDocument document, IXamlSemanticModel semanticModel, IXamlPlatform platform)
        {
            var root = document.GetSyntaxTree()?.Root;

            var targets = new List<ILocalisableString>();

            if (root != null)
            {
                var temp = CollectTargetsForNode(root, document, semanticModel, platform);

                if (temp != null && temp.Any())
                {
                    targets.AddRange(temp);
                }
            }

            return targets;
        }

        public IEnumerable<ILocalisableString> CollectTargetsForNode(XmlNode syntax, IParsedXmlDocument document, IXamlSemanticModel semanticModel, IXamlPlatform platform)
        {
            var targets = new List<ILocalisableString>();

            if (syntax.HasAttributes)
            {
                var candidates = syntax.Attributes.Where( a => CanLocalise(a, semanticModel, platform));

                foreach (var attr in candidates)
                {
                    if (attr.HasValue)
                    {
                        var target = CreateLocalisableString(attr, document);
                        targets.Add(target);
                    }
                }
            }

            if (syntax.HasChildren)
            {
                foreach (var child in syntax.Children)
                {
                    var temp = CollectTargetsForNode(child, document, semanticModel, platform);

                    if (temp != null && temp.Any())
                    {
                        targets.AddRange(temp);
                    }
                }
            }

            return targets;
        }

        protected LocalisableString CreateLocalisableString(XmlAttribute attribute, IParsedXmlDocument document)
        {
            var unescapedValue = attribute.Value.Value.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
            var target = new LocalisableString(unescapedValue, document.FilePath, attribute.Value.Span);
            return target;
        }

        public bool CanLocalise(XmlAttribute syntax, IXamlSemanticModel semanticModel, IXamlPlatform platform)
        {
            var propertySymbol = semanticModel.GetSymbol(syntax) as IPropertySymbol;
            if (propertySymbol == null)
            {
                return false;
            }

            var expression = semanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return false;
            }

            var bindablePropertyName = syntax.Name.LocalName + "Property";
            var attachedPropertySymbol = propertySymbol.ContainingType.GetMembers(bindablePropertyName);
            if (!attachedPropertySymbol.Any())
            {
                return false;
            }

            if (propertySymbol.Type.SpecialType != SpecialType.System_String)
            {
                return false;
            }

            if (propertySymbol.Name == "AutomationId" && SymbolHelper.DerivesFrom(propertySymbol.ContainingType, platform.Element.MetaType))
            {
                return false;
            }

            return true;
        }

        public bool IsAvailable(Project project, string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || project == null)
            {
                return false;
            }

            var extension = Path.GetExtension(filePath);

            if (!extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
