using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.DataBinding;
using MFractor.Maui.CodeGeneration.ValueConversion;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions
{
    class CreateResolvingValueConverterFix : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(BindingExpressionReturnTypeDoesNotMatchExpectedTypeAnalysis);

        public override string Documentation => "Creates a new value converter that converts the binding expresions result to the property's type.";

        public override string Identifier => "com.mfractor.code_fixes.xaml.create_resolving_value_converter";

        public override string Name => "Create Value Converter To Resolve Type Flow";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public IValueConverterGenerator ValueConverterGenerator { get; set; }

        [Import]
        public IValueConversionSettings ValueConversionSettings { get; set; }

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        readonly IMarkupExpressionEvaluater expressionEvaluater;

        [ImportingConstructor]
        public CreateResolvingValueConverterFix(IMarkupExpressionEvaluater expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        bool TryResolveInputAndOutputType(XmlAttribute syntax, IXamlFeatureContext context, out ITypeSymbol input, out ITypeSymbol output)
        {
            input = null;
            output = null;

            var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax);
            if (parentSymbol == null)
            {
                return false;
            }

            output = SymbolHelper.ResolveMemberReturnType(parentSymbol) as ITypeSymbol;
            if (output == null)
            {
                return false;
            }

            var binding = context.XamlSemanticModel.GetExpression(syntax) as BindingExpression;
            if (binding == null)
            {
                return false;
            }

            XamlSymbolInfo resolvedType;
            resolvedType = expressionEvaluater.EvaluateDataBindingExpression(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, binding);
            if (resolvedType == null || resolvedType.Symbol as ISymbol == null)
            {
                return false;
            }

            input = binding.ReferencesBindingContext ? resolvedType.Symbol as ITypeSymbol : SymbolHelper.ResolveMemberReturnType(resolvedType.Symbol as ISymbol);

            return true;
        }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return TryResolveInputAndOutputType(syntax, context, out var input, out var output);
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, Xml.XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            TryResolveInputAndOutputType(syntax, context, out var input, out var output);

            return CreateSuggestion($"Create a new value converter to convert '{input.Name}' to '{output.Name}'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, Xml.XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            TryResolveInputAndOutputType(syntax, context, out var input, out var output);

            IReadOnlyList<IWorkUnit> importValueConverterCallback(string valueConverterSymbolName)
            {
                var root = document.GetSyntaxTree().Root;
                var xmlPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

                var workUnits = new List<IWorkUnit>();

                SymbolHelper.ExplodeTypeName(valueConverterSymbolName, out var importNamespace, out var name);

                var resourceKey = name.FirstCharToLower();

                var binding = context.XamlSemanticModel.GetExpression(syntax) as BindingExpression;

                var valueExpressionEnd = binding.BindingValue.Span.End;

                var content = ", Converter={" + context.Platform.StaticResourceExtension + " " + resourceKey + "}";

                workUnits.Add(new ReplaceTextWorkUnit(document.FilePath, content, new Microsoft.CodeAnalysis.Text.TextSpan(valueExpressionEnd, 0)));

                return workUnits;
            }

            var defaultInput = input.Name + "To" + output.Name + "Converter";

            return new ValueConverterWizardWorkUnit()
            {
                TargetProject = context.Project?.GetIdentifier(),
                Platform = context.Platform,
                InputType = input.ToString(),
                OutputType = output.ToString(),
                AutomaticTypeInference = false,
                CreateXamlDeclaration = true,
                TargetFiles = new List<IProjectFile>() { document.ProjectFile },
                ValueConverterName = defaultInput,
                OnConverterGenerated = importValueConverterCallback
            }.AsList();
        }
    }
}
