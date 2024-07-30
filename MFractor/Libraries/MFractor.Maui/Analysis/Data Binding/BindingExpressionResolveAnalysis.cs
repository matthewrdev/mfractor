using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.DataBinding
{
    class BindingExpressionResolveAnalysis : ExpressionTypeAnalysisRoutine<BindingExpression>
    {
        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        readonly Lazy<IBindingContextResolver> bindingContextResolver;
        public IBindingContextResolver BindingContextResolver => bindingContextResolver.Value;

        [ImportingConstructor]
        public BindingExpressionResolveAnalysis(Lazy<IBindingContextResolver> bindingContextResolver,
                                                Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.bindingContextResolver = bindingContextResolver;
            this.expressionEvaluater = expressionEvaluater;
        }

        public override string Documentation => "Inspects data binding expressions and validates that the symbol referred to in the binding context exists. This analyser requires an explict or implicit binding context.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.binding_expression_resolves";

        public override string Name => "Binding Expressions Resolve";

        public override string DiagnosticId => "MF1009";

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(BindingExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var bindingContext = context.XamlSemanticModel.GetBindingContext(expression, context);
            if (bindingContext == null)
            {
                return null;
            }

            if (!expression.HasReferencedSymbol)
            {
                return null; // Can't evalaute.
            }

            var symbol = context.XamlSemanticModel.GetDataBindingExpressionResult(expression, context) as IPropertySymbol;
            if (symbol != null)
            {
                return null;
            }

            if (expression.ReferencesBindingContext)
            {
                return null;
            }

            var bindingContextReferencePrefix = context.Platform.BindingContextProperty + ".";

            // If unresolved, check the path that is referenced by the expression and figure out what didn't resolve. 
            var symbolPathValue = expression.ReferencedSymbolValue;

            // Can't handle array notation at the moment...
            if (symbolPathValue.Contains("[") || symbolPathValue.Contains("]"))
            {
                return null;
            }

            var prefix = string.Empty;

            if (symbolPathValue.Contains("."))
            {
                if (symbolPathValue.StartsWith(bindingContextReferencePrefix) && ExpressionEvaluater.UsesCodeBehindReference(expression))
                {
                    prefix = bindingContextReferencePrefix;
                    symbolPathValue = symbolPathValue.Remove(0, bindingContextReferencePrefix.Length);

                    bindingContext = ExpressionEvaluater.LocateCodeBehindReferenceBindingContextTypeForExpression(expression, context);

                    if (bindingContext is null)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            var message = $"The path '{symbolPathValue}' does not resolve to a property in the {context.Platform.BindingContextProperty} '{bindingContext.Name}'.";

            var symbolPath = new List<ISymbol>();
            ISymbol bestMatch = null;
            var suggestionPath = "";
            if (symbolPathValue.Contains("."))
            {
                var parentSymbol = bindingContext;

                // Explode into components
                var components = symbolPathValue.Split('.');

                for (var i = 0; i < components.Length; ++i)
                {
                    var value = components[i];
                    var tempSymbol = SymbolHelper.ResolveNearestNamedProperty(parentSymbol, value);
                    if (tempSymbol != null)
                    {
                        symbolPath.Add(tempSymbol);

                        parentSymbol = SymbolHelper.ResolveMemberReturnType(tempSymbol);
                    }
                    else
                    {
                        break;
                    }
                }

                if (symbolPath.Count == components.Length)
                {
                    suggestionPath = string.Join(".", symbolPath.Select(s => s.Name));
                }
                else
                {
                    if (symbolPath.Count > 0)
                    {
                        for (var i = 0; i < components.Length; ++i)
                        {
                            if (i > 0)
                            {
                                suggestionPath += ".";
                            }

                            if (i < symbolPath.Count)
                            {
                                suggestionPath += symbolPath[i].Name;
                            }
                            else
                            {
                                suggestionPath += components[i];
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(suggestionPath))
                {
                    message += $"\n\nDid you mean '{suggestionPath}'?";
                }
            }
            else
            {
                // Try find the best matching member...
                bestMatch = SymbolHelper.ResolveNearestNamedProperty(bindingContext, symbolPathValue);

                if (bestMatch != null)
                {
                    message += $"\n\nDid you mean '{bestMatch.Name}'?";
                }
            }

            var bundle = new BindingAnalysisBundle()
            {
                Expression = expression,
                SuggestedSymbol = bestMatch,
                SymbolPath = symbolPath,
                SuggestedPath = suggestionPath,
                Prefix = prefix,
            };

            return CreateIssue(message, expression.ParentAttribute, expression.ReferencedSymbolSpan, bundle).AsList();
        }
    }
}
