using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.DataBinding;
using MFractor.Maui.CodeGeneration.Commands;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeActions
{
    class GenerateBindingCommandStubFix : FixCodeAction
	{
        public override string Documentation => "When the symbol referenced inside a binding expression is unresolved, this fix can generates an ICommand implementation onto the views BindingContext";

        public override Type TargetCodeAnalyser => typeof(BindingExpressionResolveAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.generate_binding_command_stub";

        public override string Name => "Generate Missing Binding Command Stub";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public ICommandImplementationGenerator CommandGenerator { get; set; }

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

            var symbol = context.XamlSemanticModel.GetSymbol(syntax);

            var parentType = SymbolHelper.ResolveMemberReturnType(symbol);

            if (parentType == null)
            {
                return false;
            }

            return parentType.ToString() == "System.Windows.Input.ICommand";
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue,
                                                                     XmlAttribute syntax,
                                                                     IParsedXamlDocument document,
                                                                     IXamlFeatureContext context,
                                                                     InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<BindingAnalysisBundle>();
            var expression = content.Expression;

            var bindingContext = context.XamlSemanticModel.GetBindingContext(expression, context);

            return CreateSuggestion($"Generate a new command named {expression.ReferencedSymbolValue}' in {bindingContext.ToString()}", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue,
                                                          XmlAttribute syntax,
                                                          IParsedXamlDocument document,
                                                          IXamlFeatureContext context,
                                                          ICodeActionSuggestion suggestion,
                                                          InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<BindingAnalysisBundle>();

            var generationTarget = context.XamlSemanticModel.GetBindingContext(content.Expression, context);
            var targetReturnType = SymbolHelper.ResolveMemberReturnType(context.XamlSemanticModel.GetSymbol(syntax));

            var memberName = "";

            ClassDeclarationSyntax classSyntax = null;

            var declaringSyntax = generationTarget.DeclaringSyntaxReferences.First();

            classSyntax = declaringSyntax.GetSyntax() as ClassDeclarationSyntax;

            var components = content.Expression.ReferencedSymbolValue.Split('.');
            memberName = components.Last();

            var options = FormattingPolicyService.GetFormattingPolicy(context);

            var targetProject = context.Project.Solution.Projects.FirstOrDefault(p => p.AssemblyName == generationTarget.ContainingAssembly.Name);

            var members = CommandGenerator.GenerateSyntax(memberName, context.Platform.Command.MetaType);

            var workUnit = new InsertSyntaxNodesWorkUnit
            {
                HostNode = classSyntax,
                SyntaxNodes = members.ToList(),
                Workspace = context.Workspace,
                Project = targetProject
            };

            return workUnit.AsList();
        }
	}
}

