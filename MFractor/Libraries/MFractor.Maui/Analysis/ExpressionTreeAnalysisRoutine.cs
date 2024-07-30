using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;

namespace MFractor.Maui.Analysis
{
    /// <summary>
    /// An expression tree analysis routine provides 
    /// </summary>
    public abstract class ExpressionTreeAnalysisRoutine<TExpression> : XamlExpressionAnalysisRoutine where TExpression : Expression
    {
        /// <summary>
        /// Given the <paramref name="expression"/> and <paramref name="expressionDepth"/>, should this expression analyser continue to recursively inspect its children?
        /// </summary>
        /// <returns><c>true</c>, if inspect expression children was shoulded, <c>false</c> otherwise.</returns>
        /// <param name="expression">Expression.</param>
        /// <param name="expressionDepth">Expression depth.</param>
        public abstract bool ShouldInspectExpressionChildren(Expression expression, int expressionDepth);

        /// <summary>
        /// Analyse the specified expression, document and context.
        /// </summary>
        /// <returns>The analyse.</returns>
        /// <param name="expression">Expression.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        public sealed override IReadOnlyList<ICodeIssue> Analyse(Expression expression, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return Analyse(expression, document, context, 1);
        }

        /// <summary>
        /// Analyse the specified expression, document, context and depth.
        /// </summary>
        /// <returns>The analyse.</returns>
        /// <param name="expression">Expression.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="depth">Depth.</param>
        public virtual IReadOnlyList<ICodeIssue> Analyse(Expression expression, IParsedXamlDocument document, IXamlFeatureContext context, int depth)
        {
            var results = new List<ICodeIssue>();

            if (expression is TExpression toAnalyse)
            {
                var temp = AnalyseExpression(toAnalyse, document, context);
                if (temp != null
                    && temp.Any())
                {
                    results.AddRange(temp);
                }
            }

            if (expression.HasChildren
                && ShouldInspectExpressionChildren(expression, depth))
            {
                var nextDepth = depth + 1;
                foreach (var c in expression.Children)
                {
                    var temp = Analyse(c, document, context, nextDepth);
                    if (temp != null
                        && temp.Any())
                    {
                        results.AddRange(temp);
                    }
                }
            }

            if (expression is PropertyAssignmentExpression assignment 
                && assignment.AssignmentValue != null)
            {
                var nextDepth = depth + 1;
                foreach (var c in assignment.AssignmentValue.Children)
                {
                    var temp = Analyse(c, document, context, nextDepth);
                    if (temp != null
                        && temp.Any())
                    {
                        results.AddRange(temp);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Analyses the expression.
        /// </summary>
        /// <returns>The expression.</returns>
        /// <param name="expression">Expression.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        public abstract IReadOnlyList<ICodeIssue> AnalyseExpression(TExpression expression, IParsedXamlDocument document, IXamlFeatureContext context);
    }
}

