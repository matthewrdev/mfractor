using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Generate
{
    class AddResourceDictionary : GenerateXamlCodeAction
    {
        public override string Documentation => "The Generate Resource Dictionary code action adds a MyView.ResourceDictionary property with a nested resource dictionary to any Xaml node that derives from VisualElement or is the root application class. Developers can quickly add a resource dictionary in just a few keystrokes; this is especially useful when you need to add a resource dictionary to the root xaml node on a control or page.";

        public override string Identifier => "com.mfractor.code_actions.xaml.generate_resource_dictionary";

        public override string Name => "Generate Resource Dictionary";

        public override DocumentExecutionFilter Filter => XmlExecutionFilters.XmlNode;

        const string resourcesPropertyName = "Resources";

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var nodeType = context.XamlSemanticModel.GetSymbol(syntax) as ITypeSymbol;
            if (nodeType == null)
            {
                return false;
            }

            if (syntax.IsRoot == false)
            {
                return false;
            }

            var isValidSymbol = SymbolHelper.DerivesFrom(nodeType, context.Platform.VisualElement.MetaType)
                                 || SymbolHelper.DerivesFrom(nodeType, context.Platform.Application.MetaType);
            if (!isValidSymbol)
            {
                return false;
            }

            // Is the user assigning via an attribute?
            if (syntax.HasAttribute(resourcesPropertyName))
            {
                return false;
            }

            var platformXmlns = context.Namespaces.ResolveNamespaceForSchema(context.Platform.SchemaUrl);
            if (platformXmlns == null)
            {
                return false;
            }

            var propertyName = syntax.Name.FullName + "." + resourcesPropertyName;

            if (syntax.HasChildNamed(propertyName))
            {
                return false;
            }

            return true;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Add resource dictionary", 0).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var resourcePropertyNode = new XmlNode
            {
                Name = new XmlName(syntax.Name.FullName + "." + resourcesPropertyName),
                IsSelfClosing = false
            };

            var xmlns = context.Namespaces.ResolveNamespaceForSchema(context.Platform.SchemaUrl);
            var resourceDictionaryNodeName = string.IsNullOrEmpty(xmlns.Prefix) ? context.Platform.ResourceDictionary.Name : xmlns.Prefix + ":" + context.Platform.ResourceDictionary.Name;

            var resourceDictionaryNode = new XmlNode
            {
                Name = new XmlName(resourceDictionaryNodeName),
                IsSelfClosing = false
            };

            resourcePropertyNode.AddChildNode(resourceDictionaryNode);

            return new InsertXmlSyntaxWorkUnit(resourcePropertyNode, syntax, document.FilePath).AsList();
        }
    }
}
