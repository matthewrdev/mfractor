using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.ContentPage;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    class EncapsulateContentPageChildrenWithStackLayoutFix : FixCodeAction
    {
        public override string Documentation => "When a content page has multiple direct children, this fix merges them into a grid.";

        public override Type TargetCodeAnalyser => typeof(ContentPageHasMultipleChildrenAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.encapsulate_content_page_children_with_stack_layout";

        public override string Name => "Encapsulate Content Page Children With StackLayout";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Encapsulate children with {context.Platform.StackLayout.Name}").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var children = syntax.GetChildren(c => !XamlSyntaxHelper.IsPropertySetter(c));
            var properties = syntax.GetChildren(c => XamlSyntaxHelper.IsPropertySetter(c));

            var hostNode = syntax.GetChildNode(c => c.Name.LocalName == "ContentPage.Content") ?? syntax;

            var newNode = syntax.Clone(false, hostNode == syntax);

            newNode.Children = new List<XmlNode>();

            if (hostNode == syntax)
            {
                newNode.Children.AddRange(properties);
            }

            var xamlNamespace = context.Namespaces.ResolveNamespaceForSchema(context.Platform.SchemaUrl);

            var stackLayout = new XmlNode();
            stackLayout.Name = new XmlName(xamlNamespace.Prefix, context.Platform.StackLayout.Name);
            stackLayout.Children = children;

            newNode.AddChildNode(stackLayout);

            var defaultPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var policy = new OverloadableXmlFormattingPolicy(defaultPolicy)
            {
                MaxAttributesPerLine = 1
            };

            return new ReplaceXmlSyntaxWorkUnit()
            {
                New = newNode,
                Existing = hostNode,
                FilePath = document.FilePath,
                ReplaceChildren = true,
                GenerateClosingTags = true,
                FormattingPolicy = policy
            }.AsList();
        }
    }
}

