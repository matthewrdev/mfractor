using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code;
using MFractor.Maui.CodeGeneration.Grids;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;

namespace MFractor.Maui.CodeActions
{
    class EncapsulateScrollViewChildrenWithGridFix : FixCodeAction
    {
        public override string Documentation => "When a scroll view has multiple direct children, this fix merges them into a grid.";

        public override Type TargetCodeAnalyser => typeof(Analysis.ScrollViewHasMultipleDirectChildrenAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.encapsulate_scroll_view_children_with_grid";

        public override string Name => "Encapsulate Scroll View Children With Grid";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
        public IGridGenerator GridGenerator { get; set; }

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Encapsulate children with {context.Platform.Grid.Name}", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var respons = new List<IWorkUnit>();

            var children = syntax.GetChildren(c => !XamlSyntaxHelper.IsPropertySetter(c));
            var properties = syntax.GetChildren(c => XamlSyntaxHelper.IsPropertySetter(c));

            var hostNode = syntax.GetChildNode(c => c.Name.LocalName == "ScrollView.Content") ?? syntax;

            var newNode = syntax.Clone(false, hostNode == syntax);

            newNode.Children = new List<XmlNode>();

            if (hostNode == syntax)
            {
                newNode.Children.AddRange(properties);
            }

            var grid = GridGenerator.GenerateSyntax(context, children);

            newNode.AddChildNode(grid);

            var xmlPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var policy = new OverloadableXmlFormattingPolicy(xmlPolicy);
            policy.MaxAttributesPerLine = 1;

            return new ReplaceXmlSyntaxWorkUnit()
            {
                New = newNode,
                Existing = hostNode,
                FilePath = document.FilePath,
                ReplaceChildren = true,
                GenerateClosingTags = true,
                FormattingPolicy = policy,
            }.AsList();
        }
    }
}

