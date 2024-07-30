using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;

namespace MFractor.Maui.Analysis
{
    public abstract class ExpressionTypeAnalysisRoutine<TExpression> : XamlExpressionAnalysisRoutine where TExpression : Expression
	{
        public sealed override IReadOnlyList<ICodeIssue> Analyse(Expression expression, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var casted = expression as TExpression;
			if (casted == null)
			{
				return null;
			}

			return AnalyseExpression(casted, document, context);
		}

        public abstract IReadOnlyList<ICodeIssue> AnalyseExpression (TExpression expression, IParsedXamlDocument document, IXamlFeatureContext context);
	}
}

