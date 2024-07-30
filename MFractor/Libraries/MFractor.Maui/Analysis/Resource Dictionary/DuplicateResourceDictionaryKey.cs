using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.ResourceDictionary
{
    class DuplicateResourceDictionaryKey : XamlCodeAnalyser
	{
        public override string Documentation => "Validates the each resource entry within a resource dictionary has a unique key.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.duplicate_resource_dictionary_key";

        public override string Name => "Duplicate Resource Dictionary Keys";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1042";

        protected override bool IsInterestedInXamlDocument(IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var microsoftNamespace = context.Namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);

			if (microsoftNamespace == null)
			{
				return false;
			}

			return true;
		}

		protected override IReadOnlyList<ICodeIssue> Analyse (XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var symbol = context.XamlSemanticModel.GetSymbol(syntax) as ITypeSymbol;
            if (symbol == null)
			{
				return null;
			}

			var resourceDictionaryMetaType = context.Platform.ResourceDictionary.MetaType;

			if (SymbolHelper.DerivesFrom(symbol, resourceDictionaryMetaType) == false)
			{
				return null;
			}

			// Is this a property setter?
			if (XamlSyntaxHelper.IsPropertySetter(syntax))
			{
				// Don't validate resource dictionary property setters.
				return null;
			}

			if (!syntax.HasChildren)
			{
				return null;
			}

            var microsoftNamespace = context.Namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);

			var keyNameAttr = $"{microsoftNamespace.Prefix}:{Keywords.MicrosoftSchema.DictionaryKey}";

			var keyedAttrs = syntax.Children.Select(node => node.GetAttributeByName(keyNameAttr)).Where((arg) => arg != null).ToList();

			if (keyedAttrs.Count == 0)
			{
				return null;
			}

			var distinct = keyedAttrs.DistinctBy(attr => attr.Value);

			if (distinct.Count() == keyedAttrs.Count)
			{
				return null;
			}

            return CreateIssue("This resource dictionary contains duplicate keys.", syntax, syntax.NameSpan).AsList();
		}
	}
}
