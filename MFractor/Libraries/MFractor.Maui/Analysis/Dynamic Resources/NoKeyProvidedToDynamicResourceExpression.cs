using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;

namespace MFractor.Maui.Analysis.DynamicResources
{
    class NoKeyProvidedToDynamicResourceExpression : ExpressionTypeAnalysisRoutine<DynamicResourceExpression>
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.no_key_provided_to_dynamic_resource";

        public override string Name => "No Key Provided To DynamicResource Expression";

        public override string Documentation => "Inspects `DynamicResource` expressions and validates that a resource key has been provided.";

        public override string DiagnosticId => "MF1011";

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(DynamicResourceExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var value = expression.Value;
            if (value != null && value.HasValue)
            {
                return null;
            }

            return CreateIssue("No key provided to this DynamicResource expression.", expression.ParentAttribute, expression.Span).AsList();
        }
    }
}
