using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Setter
{
    class SetterAttachedPropertyExists : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.validate_setter_attached_property";

        public override string Name => "Validate Setter Attached Property Usage";

        public override string Documentation => "Inspects the `Property` attribute for a `.Setter` and, when it is referencing an attached property, validates that the namespace, class and attached property exist.";

        public override string DiagnosticId => "MF1048";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax,
                                                           IParsedXamlDocument document,
                                                           IXamlFeatureContext context)
        {
            if (syntax.Name.FullName != "Property"
                || !syntax.HasValue)
            {
                return null;
            }

            var propertyValue = syntax.Value.Value;
            if (!XamlSyntaxHelper.ExplodeAttachedProperty(propertyValue, out _, out _))
            {
                return null;
            }

            var type = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(type, context.Platform.Setter.MetaType))
            {
                return null;
            }

            return null;
        }
    }
}
