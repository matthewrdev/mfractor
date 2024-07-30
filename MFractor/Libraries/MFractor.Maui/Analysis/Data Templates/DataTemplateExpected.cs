using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.DataTemplates
{
    class DataTemplateExpected : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects property nodes that accept a data template and verifies that the content is a DataTemplate.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.data_template_expected";

        public override string Name => "Data Template Expected";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1098";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.PropertySetterNodeExecutionFilter;

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (!XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                return null;
            }

            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
            var returnType = SymbolHelper.ResolveMemberReturnType(property);
            if (!SymbolHelper.DerivesFrom(returnType, context.Platform.DataTemplate.MetaType))
            {
                return null;
            }

            var child = syntax.Children?.FirstOrDefault();
            if (child is null)
            {
                return null;
            }

            var childType = context.XamlSemanticModel.GetSymbol(child) as INamedTypeSymbol;
            if (childType == null)
            {
                return null;
            }

            if (SymbolHelper.DerivesFrom(childType, context.Platform.DataTemplate.MetaType))
            {
                return null;
            }

            return CreateIssue($"{syntax.Name.FullName} expects a DataTemplate as it's input. Did you mean to wrap this element in a DataTemplate?", child, child.Span).AsList();
        }
    }
}

