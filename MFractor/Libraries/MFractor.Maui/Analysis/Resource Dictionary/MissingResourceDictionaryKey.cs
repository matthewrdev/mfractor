using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.ResourceDictionary
{
    class MissingResourceDictionaryKey : XamlCodeAnalyser
	{
        public override string Documentation => "Validates the elements provided to a resource dictionary supply an `x:Key` attribute to declare their resource dictionary key.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.missing_resource_dictionary_key";

        public override string Name => "Resource Entry Does Not Define Key";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1043";

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
			if (syntax.IsRoot)
            {
                return null;
            }

			var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent) as ITypeSymbol;
            if (parentSymbol == null)
            {
                return null;
            }

			if (SymbolHelper.DerivesFrom(parentSymbol, context.Platform.ResourceDictionary.MetaType) == false)
            {
                return null;
            }

			// Is this a property setter?
			if (XamlSyntaxHelper.IsPropertySetter(syntax))
			{
                // Don't validate resource dictionary property setters.
                return null;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as ITypeSymbol;
            if (SymbolHelper.DerivesFrom(symbol, context.Platform.Style.MetaType))
            {
                return null;
            }

			var hasResourceKey = syntax.HasAttribute(attr =>
			{
				if (attr.Name.HasNamespace == false)
				{
					return false;
				}

                var xmlns = context.Namespaces.ResolveNamespace(attr);
				if (!XamlSchemaHelper.IsMicrosoftSchema(xmlns))
				{
					return false;
				}

				return attr.Name.LocalName == Keywords.MicrosoftSchema.DictionaryKey;
			});

			if (hasResourceKey)
			{
                return null;
			}

            return CreateIssue("Resources in a resource dictionary require an x:Key attribute.", syntax, syntax.NameSpan).AsList();
		}
	}
}
