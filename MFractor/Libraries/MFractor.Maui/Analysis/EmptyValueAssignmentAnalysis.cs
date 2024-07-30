using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class EmptyValueAssignmentAnalysis : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.empty_value_assignment";

        public override string Name => "Empty Value Assignment";

        public override string Documentation => "Detects when a boolean, double, long or integer value is being assigned an empty value and will cause a compilation error.";

        public override string DiagnosticId => "MF1070";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, 
                                                           IParsedXamlDocument document, 
                                                           IXamlFeatureContext context)
        {
            if (syntax.HasValue)
            {
                return null;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(syntax);

            var returnType = SymbolHelper.ResolveMemberReturnType(symbol);
            if (returnType == null 
                || returnType.SpecialType == SpecialType.System_String)
            {
                return null;
            }

            if (!returnType.IsValueType)
            {
                return null;
            }

            return CreateIssue("The member " + syntax.Name + " is a value type (" + returnType.ToString() + ") and is being assigned an empty value. This may cause a runtime or compilation error.", syntax, syntax.Span).AsList();
        }
    }
}
