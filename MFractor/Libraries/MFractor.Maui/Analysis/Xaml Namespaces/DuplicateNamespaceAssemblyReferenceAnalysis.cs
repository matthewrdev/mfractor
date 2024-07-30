using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.XamlNamespaces
{
    class DuplicateNamespaceAssemblyReferenceAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "Checks that an xml namespace points to a unique namespace and assembly. For example if both `xmlns:local=\"clr-namespace:MFractor.Licensing.MobileApp\"` and `xmlns:myassembly=\"clr-namespace:MFractor.Licensing.MobileApp\"` were declared, this analyser would warn that they both reference the same assembly and namespace.";

        public override IssueClassification Classification => IssueClassification.Improvement;

        public override string Identifier => "com.mfractor.code.analysis.xaml.multiple_namespace_assembly_references";

        public override string Name => "Duplicate Namespace Declaration";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1066";

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (!syntax.Parent.IsRoot)
            {
                return null;
            }

            var isNamespace = (syntax.Name.HasNamespace ? syntax.Name.Namespace : syntax.Name.LocalName) == "xmlns";

            if (!isNamespace)
            {
                return null;
            }


            var namespaceCount = 0;
            foreach (var ns in context.Namespaces.Namespaces)
            {
                if (ns.Value == syntax.Value?.Value)
                {
                    namespaceCount++;
                }
            }

            if (namespaceCount <= 1)
            {
                return null;
            }

            return CreateIssue($"The namespace and assembly '{syntax.Value}' is referenced multiple times by different namespace declarations", syntax, syntax.Span).AsList();
        }

    }
}

