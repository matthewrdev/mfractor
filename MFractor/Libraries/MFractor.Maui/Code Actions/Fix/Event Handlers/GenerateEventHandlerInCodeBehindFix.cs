using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.Analysis.EventHandlers;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    class GenerateEventHandlerInCodeBehindFix : FixCodeAction
    {
        public override string Documentation => "Use the Generate Event Handler code fix to generate an event handler method on a XAML files code behind class.";

        public override Type TargetCodeAnalyser => typeof(EventHandlerDoesNotExistInCodeBehindAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.generate_event_handler";

        public override string Name => "Generate Event Handler";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public IEventHandlerMethodGenerator EventHandlerGenerator { get; set; }

        protected override bool IsAvailableInDocument(IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return document.CodeBehindSyntax != null;
        }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (!syntax.HasValue)
            {
                return false;
            }
            
            var bundle = issue.GetAdditionalContent<EventHandlerDoesNotExistInCodeBehindBundle>();

            return bundle != null && bundle.EventSymbol != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Generate a method named '" + syntax.Value + "' in code behind class.", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var eventSymbol = issue.GetAdditionalContent<EventHandlerDoesNotExistInCodeBehindBundle>().EventSymbol;

            var members = EventHandlerGenerator.GenerateSyntax(eventSymbol, syntax.Value.Value);

            var codeBehindSyntax = document.CodeBehindSyntax;

            var targetProject = SymbolHelper.GetProjectForSymbol(context.Solution, document.CodeBehindSymbol);

            var workUnit = new InsertSyntaxNodesWorkUnit
            {
                HostNode = codeBehindSyntax,
                SyntaxNodes = members.ToList(),
                Workspace = context.Workspace,
                Project = targetProject
            };

            return workUnit.AsList();
        }
    }
}
