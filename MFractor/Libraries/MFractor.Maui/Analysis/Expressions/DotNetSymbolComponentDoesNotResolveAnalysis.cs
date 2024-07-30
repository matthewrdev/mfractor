using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Analysis
{
    class DotNetSymbolComponentDoesNotResolveAnalysis : ExpressionTreeAnalysisRoutine<DotNetTypeSymbolExpression>
	{
        public override string Documentation => "Inspects a .net symbol reference (eg `local:MyClass.MyProperty`) and validates that the symbol portion ('MyClass.MyProperty') resolves to a .NET symbol within the current project and it's references.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.dotnet_symbol_resolves";

        public override string Name => "Unresolved .NET Symbols Within Xaml Expression";

        public override string DiagnosticId => "MF1018";

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

		readonly Lazy<Symbols.IXamlTypeResolver> xamlTypeResolver;
        public Symbols.IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

		readonly Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver;
		public IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver => xmlnsNamespaceSymbolResolver.Value;

		[ImportingConstructor]
        public DotNetSymbolComponentDoesNotResolveAnalysis(Lazy<IMarkupExpressionEvaluater> expressionEvaluater,
														   Lazy<Symbols.IXamlTypeResolver> xamlTypeResolver,
														   Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver)
        {
            this.expressionEvaluater = expressionEvaluater;
            this.xamlTypeResolver = xamlTypeResolver;
            this.xmlnsNamespaceSymbolResolver = xmlnsNamespaceSymbolResolver;
        }

        public override IReadOnlyList<ICodeIssue> AnalyseExpression (DotNetTypeSymbolExpression expression, 
                                                                   IParsedXamlDocument document,
                                                                   IXamlFeatureContext context)
		{
            var result = ExpressionEvaluater.EvaluateDotNetSymbolExpression(context.Project, context.Platform, context.Namespaces, context.XmlnsDefinitions, expression);
			if (result != null
				&& result.Symbol != null)
			{
				return null;
			}

            if (expression.ParentExpression != null)
            {
                return null; // HACK: This stops whacked out warnings everywhere in expressions.
            }

			if (!expression.HasSymbol)
			{
				return null;
			}

            var xamlNamespace = context.Namespaces.ResolveNamespace(expression.Namespace);

			if (xamlNamespace == null
				|| XamlSchemaHelper.IsSchema(xamlNamespace, XamlSchemas.MicrosoftSchemaUrl))
			{
				return null; // Can't resolve without the namespace portion.
			}

			var outerSymbol = context.XamlSemanticModel.GetSymbol(expression.ParentAttribute) as IPropertySymbol;

            var symbolClassRegion = expression.SymbolSpan;
			var symbolPropertyRegion = expression.SymbolSpan;

			var symbolClass = expression.Symbol;
			var symbolProperty = "";
			if (symbolClass.Contains("."))
			{
				var components = symbolClass.Split('.');

				// If more than 3 components, user trying to access 3 levels deep, this is invalid :(
				if (components.Length > 0)
				{
					symbolClass = components[0];
                    symbolClassRegion = TextSpan.FromBounds(expression.SymbolSpan.Start, expression.SymbolSpan.Start + symbolClass.Length);
				}

				if (components.Length > 1)
				{
                    var startCol = expression.SymbolSpan.Start + symbolClass.Length + 1;
					symbolProperty = components[1];
                    symbolPropertyRegion = TextSpan.FromBounds(startCol, startCol + symbolClass.Length);
				}
			}

			var type = XamlTypeResolver.ResolveType(symbolClass, xamlNamespace, context.Project, context.XmlnsDefinitions);

			var message = "";

			var symbolBundle = new DotNetSymbolComponentBundle()
			{
				Expression = expression,
				SuggestedClassName = type?.Name,
				ClassSpan = symbolClassRegion,
				ReferencedMemberName = symbolProperty,
				MemberSpan = symbolPropertyRegion
			};

			if (type == null)
			{
				message = $"No class or struct named '{symbolClass}' is declared in {xamlNamespace.Value}.";

                if (expression.HasSymbol)
				{
					var assemblies = XmlnsNamespaceSymbolResolver.GetAssemblies(xamlNamespace, context.Project, context.XmlnsDefinitions);

					foreach (var assembly in assemblies)
                    {
						var matchedSymbol = SymbolHelper.ResolveNearlyNameSymbolInAssembly(symbolClass, assembly, true);
						if (matchedSymbol != null)
						{
							message += $"\n\nDid you mean '{matchedSymbol.Name}'?";
							symbolBundle.SuggestedClassName = matchedSymbol.Name;
							break;
						}
					}
				}

                return CreateIssue(message, expression.ParentAttribute, symbolClassRegion, symbolBundle).AsList();
			}

			if (string.IsNullOrEmpty(symbolProperty))
			{
				return null; // Can't resolve without a property reference.
			}

			// Check the property...
			ISymbol nearestSymbol = null;
			if (type != null)
			{
				nearestSymbol = SymbolHelper.ResolveNearestNamedMember(type, symbolProperty);
			}

			message = $"{type} does not have a member named '{symbolProperty}'.";
			if (nearestSymbol != null)
			{
				message += $"\n\nDid you mean '{nearestSymbol.Name}'?";
				symbolBundle.SuggestedMemberName = nearestSymbol.Name;
			}

            return CreateIssue(message, expression.ParentAttribute, symbolPropertyRegion, symbolBundle).AsList();
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

