using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;

namespace MFractor.Maui.Analysis.StaticResources
{
    class NoKeyProvidedToStaticResourceExpression : ExpressionTypeAnalysisRoutine<StaticResourceExpression>
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.no_key_provided_to_static_resource";

        public override string Name => "No Key Provided To Static Resource Expression";

        public override string Documentation => "Inspects `StaticResource` expressions and validates that a resource key has been provided.";

        public override string DiagnosticId => "MF1055";

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(StaticResourceExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var value = expression.Value;
            if (value != null && value.HasValue)
            {
                return null;
            }

            return CreateIssue("No key provided to this static resource expression.", expression.ParentAttribute, expression.Span).AsList();
        }
    }
}
