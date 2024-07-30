using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.Grid;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Fix.Grids
{
    class MoveGridPropertiesToParent : FixCodeAction
    {
        public override string Documentation => "When a XAML element specifies grid rows, columns or spans, however, it is not within a grid element but it's parent is, this code fix moves all grid properties the parent XAML element.";

        public override IEnumerable<Type> TargetCodeAnalysers { get; } = new List<Type>()
        {
            typeof(RedundantColumnPropertyAnalysis),
            typeof(RedundantColumnSpanPropertyAnalysis),
            typeof(RedundantRowPropertyAnalysis),
            typeof(RedundantRowSpanPropertyAnalysis),
        };

        public override Type TargetCodeAnalyser => default;

        public override string Identifier => "com.mfractor.code_fixes.xaml.move_grid_properties_to_parent";

        public override string Name => "Move Grid Properties To Parent";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var targetProperties = GetTargetProperties(context.Platform);
            var properties = CollectGridProperties(syntax.Parent, targetProperties);

            var parent = syntax.Parent?.Parent;

            if (parent is null)
            {
                return false;
            }

            var outerParent = parent.Parent;
            var outerParentType = context.XamlSemanticModel.GetSymbol(outerParent) as ITypeSymbol;

            return properties.Any() && SymbolHelper.DerivesFrom(outerParentType, context.Platform.Grid.MetaType);
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Move grid properties to parent").AsList();
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
            var gridProperties = CollectGridProperties(syntax.Parent, targetProperties);

            var work = new List<IWorkUnit>()
            {
                new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntaxes = gridProperties,
                }
            };

            var parent = syntax.Parent.Parent;
            foreach (var property in gridProperties)
            {
                work.Add(new InsertXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    HostSyntax = parent,
                    Syntax = property
                });
            }

            return work;
        }
    }
}
