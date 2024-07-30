using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Shell
{
    class EmptyShellRouteScheme : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.empty_shell_route_scheme";

        public override string Name => "Empty Shell Route Scheme";

        public override string Documentation => "Providing an empty value into the `RouteScheme` property of a `.Shell` element will cause the application to crash. This code inspection validates that a value is provided to the RouteScheme property.";

        public override string DiagnosticId => "MF1051";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.HasValue)
            {
                return null;
            }

            if (syntax.Name.FullName != "RouteScheme")
            {
                return null;
            }

            var parentType = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(parentType, context.Platform.Shell.MetaType))
            {
                return null;
            }

            return CreateIssue("A value must be provided to the RouteScheme property; an empty value will cause a runtime crash.", syntax, syntax.Span).AsList();
        }
    }
}
