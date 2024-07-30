using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.Grid;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Grids
{
    class RemoveRedundantGridProperties : FixCodeAction
    {
        public override string Documentation => "When a XAML element specifies grid rows, columns or spans, however, it is not within a grid element, this code fix removes all redundant grid properties applied to the element.";

        public override IEnumerable<Type> TargetCodeAnalysers { get; } = new List<Type>()
        {
            typeof(RedundantColumnPropertyAnalysis),
            typeof(RedundantColumnSpanPropertyAnalysis),
            typeof(RedundantRowPropertyAnalysis),
            typeof(RedundantRowSpanPropertyAnalysis),
        };

        public override Type TargetCodeAnalyser => default;

        public override string Identifier => "com.mfractor.code_fixes.xaml.remove_redundant_grid_properties";

        public override string Name => "Remove Redundant Grid Properties";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public enum ActionKind
        {
            Single,

            All,
        }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var targetProperties = GetTargetProperties(context.Platform);
            var properties = CollectGridProperties(syntax.Parent, targetProperties);

            return properties.Any();
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return new List<ICodeActionSuggestion>()
            {
                CreateSuggestion($"Remove redundant grid properties from this element", ActionKind.Single),
                CreateSuggestion($"Remove all redundant grid properties from {syntax.Parent.Parent.Name.FullName}'s children", ActionKind.All)
            };
        }

        IReadOnlyList<string> GetTargetProperties(IXamlPlatform platform)
        {
            return new List<string>()
            {
                $"{platform.Grid.Name}.{platform.ColumnProperty}",
                $"{platform.Grid.Name}.{platform.ColumnProperty}Span",
                $"{platform.Grid.Name}.{platform.RowProperty}",
                $"{platform.Grid.Name}.{platform.RowProperty}Span",
            };
        }

        IReadOnlyList<XmlSyntax> CollectGridProperties(XmlNode node, IReadOnlyList<string> properties)
        {
            var gridProperties = new List<XmlSyntax>();

            foreach (var property in properties)
            {
                var attribute = node.GetAttributeByName(property);

                if (attribute != null)
                {
                    gridProperties.Add(attribute);
                }
            }

            return gridProperties;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue,
                                                          XmlAttribute syntax,
                                                          IParsedXamlDocument document,
                                                          IXamlFeatureContext context,
                                                          ICodeActionSuggestion suggestion,
                                                          InteractionLocation location)
        {
            var targetProperties = GetTargetProperties(context.Platform);
            var kind = suggestion.GetAction<ActionKind>();

            if (kind == ActionKind.Single)
            {
                return new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntaxes = CollectGridProperties(syntax.Parent, targetProperties),
                }.AsList();
            }
            else
            {
                var parent = syntax.Parent.Parent;

                var children = syntax.Parent.Parent.GetChildren(c => !XamlSyntaxHelper.IsPropertySetter(c));

                var deletionItems = new List<XmlSyntax>();

                foreach (var child in children)
                {
                    var result = CollectGridProperties(child, targetProperties);

                    if (result != null && result.Any())
                    {
                        deletionItems.AddRange(result);
                    }
                }

                return new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntaxes = deletionItems,
                }.AsList();
            }
        }
    }
}
