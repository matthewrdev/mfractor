using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeActions;
using MFractor.Configuration.Attributes;
using MFractor.Maui.CodeGeneration.ValueConversion;
using MFractor.Maui.Configuration;
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

namespace MFractor.Maui.CodeActions.Generate
{
    class GenerateValueConverterForTypeFlow : GenerateXamlCodeAction
    {
        public override string Documentation => "Generates a value converter to convert the return type of a binding expression into the input type of the property.";

        public override string Identifier => "com.mfractor.code_fixes.code_actions.generate_value_converter_for_type_flow";

        public override string Name => "Generate New Value Converter For Type Flow";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [ExportProperty("The namespace name of the xmlns import statement for the newly created value converter")]
        public string DefaultConverterNamespace { get; set; } = "converters";

        [Import]
        public IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        public IValueConversionSettings ValueConversionSettings { get; set; }

        public IMarkupExpressionEvaluater ExpressionEvaluater { get; }

        [ImportingConstructor]
        public GenerateValueConverterForTypeFlow(IMarkupExpressionEvaluater expressionEvaluater)
        {
            ExpressionEvaluater = expressionEvaluater;
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

            output = SymbolHelper.ResolveMemberReturnType(parentSymbol);
            if (output == null)
            {
                return false;
            }

            var binding = context.XamlSemanticModel.GetExpression(syntax) as BindingExpression;
            if (binding == null)
            {
                return false;
            }

            if (binding.Converter != null)
            {
                return false;
            }

            XamlSymbolInfo resolvedType;
            resolvedType = ExpressionEvaluater.EvaluateDataBindingExpression(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, binding);
            if (resolvedType == null || resolvedType.Symbol as ISymbol == null)
            {
                return false;
            }

            input = binding.ReferencesBindingContext ? resolvedType.Symbol as ITypeSymbol : SymbolHelper.ResolveMemberReturnType(resolvedType.Symbol as ISymbol);

            return true;
        }

        public override bool CanExecute(Xml.XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return TryResolveInputAndOutputType(syntax, context, out _, out _);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(Xml.XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            TryResolveInputAndOutputType(syntax, context, out var input, out var output);

            return CreateSuggestion($"Generate a new value converter that converts a '{input.Name}' into a '{output.Name}'", 0).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            TryResolveInputAndOutputType(syntax, context, out var input, out var output);

            IReadOnlyList<IWorkUnit> generateValueConverter(string valueConverterName)
            {
                var root = document.GetSyntaxTree().Root;
                var xmlPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

                var @namespace = ValueConversionSettings.CreateConvertersClrNamespace(context.Project.GetIdentifier());

                SymbolHelper.ExplodeTypeName(valueConverterName, out var importNamespace, out var name);

                var workUnits = new List<IWorkUnit>();

                var converterNamespaceName = this.DefaultConverterNamespace;

                var resourceKey = name.FirstCharToLower();

                var binding = context.XamlSemanticModel.GetExpression(syntax) as BindingExpression;

                var valueExpressionEnd = binding.BindingValue.Span.End;

                var content = ", Converter={" + context.Platform.StaticResourceExtension.MarkupExpressionName + " " + resourceKey + "}";

                workUnits.Add(new ReplaceTextWorkUnit(document.FilePath, content, new Microsoft.CodeAnalysis.Text.TextSpan(valueExpressionEnd, 0)));

                return workUnits;
            }

            var defaultInput = input.Name + "To" + output.Name + "Converter";

            return new ValueConverterWizardWorkUnit()
            {
                AutomaticTypeInference = false,
                InputType = input.ToString(),
                OutputType = output.ToString(),
                CreateXamlDeclaration = true,
                ValueConverterName = defaultInput,
                Platform = context.Platform,
                TargetProject = document.ProjectFile.CompilationProject.GetIdentifier(),                
                TargetFiles = new List<IProjectFile>() { document.ProjectFile },
                OnConverterGenerated = generateValueConverter,
            }.AsList();
        }
    }
}
