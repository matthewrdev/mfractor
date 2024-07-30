using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class DotNetSymbolNamespaceBundle
	{
		public string Suggestion;
		public ITypeSymbol MatchedSymbol;
		public DotNetTypeSymbolExpression Expression;
	}

	class DotNetSymbolNamespaceResolvesAnalysis : ExpressionTreeAnalysisRoutine<DotNetTypeSymbolExpression>
	{
        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        public override string Documentation => "Inspects a .net symbol reference (eg `local:MyClass.MyProperty`) and validates that the namespace portion ('local') resolves to a xmlns declaration in the current document.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.dotnet_symbol_namespace_resolves";

        public override string Name => "Unresolved Namespace Within Xaml Expression";

        public override string DiagnosticId => "MF1019";

        [ImportingConstructor]
        public DotNetSymbolNamespaceResolvesAnalysis(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(DotNetTypeSymbolExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
		{
            var result = ExpressionEvaluater.EvaluateDotNetSymbolExpression(context.Project, context.Platform, context.Namespaces, context.XmlnsDefinitions, expression);

			if (result != null 
			    && result.Symbol != null)
			{
				return null;
			}

            var xamlNamespace = context.Namespaces.ResolveNamespace(expression.Namespace);

			if (xamlNamespace != null)
			{
				return null;
			}

			var message = $"A namespace named '{expression.Namespace}' is not declared in the current document.";

			var namespaces = context.Namespaces.Namespaces.Where(n => n.Prefix != "x").Select(n => n.Prefix);

			var suggestion = SuggestionHelper.FindBestSuggestion(expression.Namespace, namespaces);

            if (suggestion == expression.Namespace)
            {
                return null;
            }

			INamedTypeSymbol matchedSymbol = null;

            if (!string.IsNullOrEmpty(suggestion) && suggestion != expression.Namespace)
            {
                message += $"\n\nDid you mean '{suggestion}'?";
            }
            else
            {
                if (expression.HasSymbol && expression.HasClassComponent)
                {
                    var symbol = SymbolHelper.ResolveSymbolInCompilation(expression.ClassName, context.Compilation);

                    if (symbol != null)
                    {
                        matchedSymbol = symbol;
                    }
                }
            }

            var bundle = new DotNetSymbolNamespaceBundle()
            {
                Expression = expression,
                Suggestion = suggestion,
                MatchedSymbol = matchedSymbol
            };

            return CreateIssue(message, expression.ParentAttribute, expression.NamespaceSpan, bundle).AsList();
		}

		public override bool ShouldInspectExpressionChildren(Expression expression, int expressionDepth)
		{
			if (expression is StringFormatExpression
			   || expression is PathExpression
		       || expression is NameExpression
			   || expression is BindingModeExpression)
			{
				return false;
			}

			return true;
		}
	}
}

