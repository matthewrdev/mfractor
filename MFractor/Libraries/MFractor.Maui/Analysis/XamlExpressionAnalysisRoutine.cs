using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Xml;

namespace MFractor.Maui.Analysis
{
    /// <summary>
    /// A <see cref="XamlExpressionAnalysisRoutine"/> inspects XAML expressions and their components.
    /// </summary>
    public abstract class XamlExpressionAnalysisRoutine : XamlCodeAnalyser
	{
        /// <summary>
        /// The xml element that this analyser should inspect; for <see cref="XamlExpressionAnalysisRoutine"/> this is always <see cref="XmlSyntaxKind.Attribute"/>.
        /// </summary>
        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.ExpressionExecutionFilter;

        /// <summary>
        /// Analyse the specified syntax, document and context.
        /// </summary>
        protected sealed override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression == null)
            {
				return null;
			}

			return Analyse (expression, document, context);
		}

        /// <summary>
        /// Analyse the specified expression, document and context.
        /// </summary>
        public abstract IReadOnlyList<ICodeIssue> Analyse (Expression expression, IParsedXamlDocument document, IXamlFeatureContext context);
	}
}

