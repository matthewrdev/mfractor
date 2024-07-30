using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.XamlNamespaces
{
    class XmlNamespaceDoesNotExistAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "Checks that the namespace used on the xml nodes is defined within the current document.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.xml_namespace_does_not_resolve";

        public override string Name => "Unresolved Xml Namespace";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1067";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (string.IsNullOrEmpty(syntax.Name.Namespace))
            {
                return null;
            }

            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax);

            if (xamlNamespace != null)
            {
                return null;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(syntax);
            if (symbol != null)
            {
                return null;
            }

            var message = $"A namespace named '{syntax.Name.Namespace}' is not declared in the current document.";

            var namespaces = context.Namespaces.Namespaces.Select(n => n.Prefix);

            var suggestion = SuggestionHelper.FindBestSuggestion(syntax.Name.Namespace, namespaces.ToList());

            if (suggestion == syntax.Name.Namespace)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(suggestion) && suggestion != syntax.Name.Namespace)
            {
                message += $"\n\nDid you mean '{suggestion}'?";
            }

            var matchedSymbol = SymbolHelper.ResolveSymbolInCompilation(syntax.Name.LocalName, context.Compilation);
            if (matchedSymbol != null)
            {
                return CreateIssue($"'{syntax.Name.FullName}' is unknown but can be resolved to '{matchedSymbol.ToString()}' inside '{matchedSymbol.ContainingAssembly.Name}'.\n\nWould you like to import this namespace and assembly?", syntax, syntax.NameSpan, matchedSymbol).AsList();
            }

            return CreateIssue(message, syntax, syntax.NameSpan, suggestion).AsList();
        }
    }
}

