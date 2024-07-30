using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.CodeGeneration.Grids;
using MFractor.Maui.Grids;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.Grids
{
    class ConvertGridRowColumnsToNodeFormat : RefactorXamlCodeAction
    {
        readonly Lazy<IGridAxisResolver> gridAxisResolver;
        public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_actions.xaml.convert_grid_row_columns_to_node_format";

        public override string Name => "Convert Grid Row/Columns To Node Format";

        public override string Documentation => "When a grids rows or columns are declared using the attribute format, this refactoring lets you convert into the xml node row/column definition format.";

        [Import]
        public IGridColumnDefinitionGenerator GridColumnDefinitionGenerator { get; set; }

        [Import]
        public IGridRowDefinitionGenerator GridRowDefinitionGenerator { get; set; }

        [ImportingConstructor]
        public ConvertGridRowColumnsToNodeFormat(Lazy<IGridAxisResolver> gridAxisResolver)
        {
            this.gridAxisResolver = gridAxisResolver;
        }

        public override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;

            if (property is null)
            {
                return false;
            }

            if (!FormsSymbolHelper.HasTypeConverterAttribute(property, context.Platform))
            {
                return false;
            }

            return SymbolHelper.DerivesFrom(property.Type, context.Platform.RowDefinitionCollection.MetaType)
                   || SymbolHelper.DerivesFrom(property.Type, context.Platform.ColumnDefinitionCollection.MetaType);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Convert to {syntax.Name} node format").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var result = new List<IWorkUnit>()
            {
                new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntax = syntax,
                }
            };

            var children = new List<XmlNode>();

            if (syntax.Name.FullName == context.Platform.RowDefinitionsProperty)
            {
                var elements = GridAxisResolver.ResolveRowDefinitions(syntax.Parent, context.Platform);

                foreach (var element in elements)
                {
                    children.Add(GridRowDefinitionGenerator.GenerateSyntax(document.Namespaces.DefaultNamespace.Prefix, element.Value.Trim()));
                }
            }
            else if (syntax.Name.FullName == context.Platform.ColumnDefinitionsProperty)
            {
                var elements = GridAxisResolver.ResolveColumnDefinitions(syntax.Parent, context.Platform);

                foreach (var element in elements)
                {
                    children.Add(GridColumnDefinitionGenerator.GenerateSyntax(document.Namespaces.DefaultNamespace.Prefix, element.Value.Trim()));
                }
            }

            result.Add(new InsertXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                HostSyntax = syntax.Parent,
                Syntax = new XmlNode(syntax.Parent.Name + "." + syntax.Name)
                {
                    Children = children
                }
            });

            return result;
        }
    }
}