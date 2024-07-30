using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.DataBinding;
using MFractor.Maui.CodeGeneration.Resources;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.CodeActions
{
    class ImportResolvingValueConverterFix : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(BindingExpressionReturnTypeDoesNotMatchExpectedTypeAnalysis);

        public override string Documentation => "Inspects for IValueConverter implementations within the project and it's references that match the value conversion flow for this binding type mismatch. IValueConverter implementations must be annotated with the `ValueConversionAttribute` to be detected by this fix.";

        public override string Identifier => "com.mfractor.code_fixes.xaml.import_resolving_value_converter";

        public override string Name => "Import Value Converter For Binding Type Flow";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        [Import]
        public IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        readonly Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver;
        IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver => xmlnsNamespaceSymbolResolver.Value;

        [ImportingConstructor]
        public ImportResolvingValueConverterFix(Lazy<IMarkupExpressionEvaluater> expressionEvaluater, Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver)
        {
            this.expressionEvaluater = expressionEvaluater;
            this.xmlnsNamespaceSymbolResolver = xmlnsNamespaceSymbolResolver;
        }

        protected override bool CanExecute(ICodeIssue issue, Xml.XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            // Locate all value converters
            var converters = GetConverters(syntax, context);

            return converters != null && converters.Any();
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, Xml.XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var converters = GetConverters(syntax, context);

            var suggestions = new List<ICodeActionSuggestion>();
            var i = 0;
            foreach (var c in converters)
            {
                suggestions.Add(CreateSuggestion("Import '" + c.Name + "' to resolve value conversion", i));
                i++;
            }

            return suggestions;
        }

        IReadOnlyList<ITypeSymbol> GetConverters(XmlAttribute syntax, IXamlFeatureContext context)
        {
            var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax);
            if (parentSymbol == null)
            {
                return null;
            }

            var outputType = SymbolHelper.ResolveMemberReturnType(parentSymbol) as INamedTypeSymbol;
            if (outputType == null)
            {
                return null;
            }

            var binding = context.XamlSemanticModel.GetExpression(syntax) as BindingExpression;
            if (binding == null)
            {
                return null;
            }

            var resolvedType = ExpressionEvaluater.EvaluateDataBindingExpression(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, binding);
            if (resolvedType == null || resolvedType.Symbol as ISymbol == null)
            {
                return null;
            }

            var inputType = binding.ReferencesBindingContext ? resolvedType.Symbol as ITypeSymbol : SymbolHelper.ResolveMemberReturnType(resolvedType.Symbol as ISymbol);

            // Locate all value converters
            var converters = ValueConverterHelper.ResolveTypedValueConvertersInCompilation(context.Compilation, context.Platform, inputType, outputType);

            return converters;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var converters = GetConverters(syntax, context);

            var converter = converters[suggestion.ActionId];

            var workUnits = new List<IWorkUnit>();

            var ast = document.GetSyntaxTree();

            var root = ast.Root;

            var namespaceName = converter.ContainingNamespace.Name;
            var matchingNamespace = default(IXamlNamespace);

            foreach (var ns in context.Namespaces)
            {
                var namespaces = XmlnsNamespaceSymbolResolver.GetNamespaces(ns, context.Project, context.XmlnsDefinitions);

                if (namespaces.Any(n => n.ToString() == converter.ContainingNamespace.ToString()))
                {
                    matchingNamespace = ns;
                    break;
                }
            }

            if (matchingNamespace == null)
            {
                var importNamespaceFix = XamlNamespaceImportGenerator.GenerateXmlnsImportAttibute(namespaceName, converter.ContainingNamespace, false);
                workUnits.Add(new InsertXmlSyntaxWorkUnit(importNamespaceFix, root, document.FilePath));
            }
            else
            {
                namespaceName = matchingNamespace.Prefix;
            }

            var resourceKey = StringExtensions.FirstCharToLower(converter.Name);
            var converterNode = new XmlNode();
            converterNode.IsSelfClosing = true;
            converterNode.Name = new XmlName(namespaceName, converter.Name);
            converterNode.AddAttribute("x:Key", resourceKey);

            workUnits.AddRange(InsertResourceEntryGenerator.Generate(context.Project, document.FilePath, ast, converterNode));

            var binding = context.XamlSemanticModel.GetExpression(syntax) as BindingExpression;

            var valueExpressionEnd = binding.BindingValue.Span.End;

            var content = ", Converter={" + context.Platform.StaticResourceExtension.MarkupExpressionName + " " + resourceKey + "}";

            var span = new TextSpan(valueExpressionEnd, 0);

            workUnits.Add(new ReplaceTextWorkUnit(document.FilePath, content, span));

            return workUnits;
        }
    }
}
