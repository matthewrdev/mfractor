using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class ObsoletePropertyUsedAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Checks for attributes that are marked as obsolete/deprecated.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.obsolete_property_used";

        public override string Name => "Obsolete Property Used";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1071";

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var symbol = context.XamlSemanticModel.GetSymbol(syntax);
            if (symbol == null)
            {
                return null;
            }

			var attrs = symbol.GetAttributes();
			if (attrs.Length == 0)
            {
                return null;
            }

			var obsoleteAttr = attrs.FirstOrDefault(a => a.AttributeClass.ToString() == "System.ObsoleteAttribute");
			if (obsoleteAttr == null)
            {
                return null;
            }

            var message = "This has been marked as deprecated.";

            if (obsoleteAttr.ConstructorArguments.Any())
            {
                message = obsoleteAttr.ConstructorArguments[0].Value.ToString();
            }

            return CreateIssue("(Deprecated) " + message, syntax, syntax.NameSpan).AsList();
        }
	}
}

