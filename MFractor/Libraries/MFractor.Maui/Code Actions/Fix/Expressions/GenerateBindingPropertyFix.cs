using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.DataBinding;
using MFractor.Maui.CodeGeneration.CSharp;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeActions
{
    class GenerateBindingPropertyFix : FixCodeAction
    {
        [ImportingConstructor]
        public GenerateBindingPropertyFix(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        public override string Documentation => "Generates the missing property binding onto the binding context for this ";

        public override Type TargetCodeAnalyser => typeof(BindingExpressionResolveAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.generate_binding_property";

        public override string Name => "Generate Binding Expression Property";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public IViewModelPropertyGenerator PropertyGenerator { get; set; }

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        protected override bool CanExecute(ICodeIssue issue,
                                           XmlAttribute syntax,
                                           IParsedXamlDocument document,
                                           IXamlFeatureContext context,
                                           InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<BindingAnalysisBundle>();
            if (content == null)
            {
                return false;
            }

            var canGenerateTarget = true;
            if (content.SymbolPath != null)
            {
                var components = content.Expression.ReferencedSymbolValue.Split('.');
                canGenerateTarget = content.SymbolPath.Count == components.Length - 1;
            }

            if (!canGenerateTarget)
            {
                return false;
            }

            var expression = content.Expression;
            var bindingContext = context.XamlSemanticModel.GetBindingContext(expression, context);
            if (bindingContext == null || bindingContext.DeclaringSyntaxReferences.Length == 0)
            {
                return false;
            }

            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue,
                                                                     XmlAttribute syntax,
                                                                     IParsedXamlDocument document,
                                                                     IXamlFeatureContext context,
                                                                     InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<BindingAnalysisBundle>();
            var expression = content.Expression;

            var suggestions = new List<ICodeActionSuggestion>();

            var bindingContext = context.XamlSemanticModel.GetBindingContext(expression, context);
            if (bindingContext != null && bindingContext.DeclaringSyntaxReferences.Length > 0)
            {
                suggestions.Add(CreateSuggestion($"Generate property named '{expression.ReferencedSymbolValue}' in '{bindingContext.MetadataName}'"));
            }

            return suggestions;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue,
                                                          XmlAttribute syntax,
                                                          IParsedXamlDocument document,
                                                          IXamlFeatureContext context,
                                                          ICodeActionSuggestion suggestion,
                                                          InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<BindingAnalysisBundle>();

            var expression = content.Expression;

            var generationTarget = context.XamlSemanticModel.GetBindingContext(expression, context);
            var targetReturnType = SymbolHelper.ResolveMemberReturnType(context.XamlSemanticModel.GetSymbol(syntax));

            if (content.Expression.Converter != null
                && content.Expression.Converter.AssignmentValue != null)
            {
                targetReturnType = ValueConverterHelper.ResolveValueConverterInputType(context, content.Expression.Converter, ExpressionEvaluater);
            }

            if (targetReturnType == null)
            {
                targetReturnType = context.Compilation.GetTypeByMetadataName("System.Object");
            }

            ClassDeclarationSyntax classSyntax = null;

            var declaringSyntax = generationTarget.DeclaringSyntaxReferences.First();

            classSyntax = declaringSyntax.GetSyntax() as ClassDeclarationSyntax;

            var components = content.Expression.ReferencedSymbolValue.Split('.');
            var memberName = components.Last();

            var targetProject = context.Project.Solution.Projects.FirstOrDefault(p => p.AssemblyName == generationTarget.ContainingAssembly.Name);

            var code = PropertyGenerator.GenerateSyntax(targetReturnType, Accessibility.Public, memberName, null);
            return new InsertSyntaxNodesWorkUnit()
            {
                HostNode = classSyntax,
                Workspace = context.Workspace,
                Project = targetProject,
                SyntaxNodes = code.Select(s => s as SyntaxNode).ToList()
            }.AsList();
        }
    }
}

