using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.CodeGeneration.Grids;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Generate
{
    class AddRowAndColumnDefinitions : GenerateXamlCodeAction
    {
        public override string Documentation => "This code action inserts a Grid.ColumnDefinitions and Grid.RowDefinitions into a Grid element.";

        public override string Identifier => "com.mfractor.code_actions.xaml.generate_grid_row_and_column_definitions";

        public override string Name => "Generate Grid Row And Column Definitions";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
        public IGridRowDefinitionGenerator RowDefinitionGenerator { get; set; }

        [Import]
        public IGridColumnDefinitionGenerator ColumnDefinitionGenerator { get; set; }

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var platform = context.Platform;
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (symbol == null)
            {
                return false;
            }

            var gridType = context.Compilation.GetTypeByMetadataName(context.Platform.Grid.MetaType);

            if (SymbolHelper.IsTypeMismatch(symbol, gridType))
            {
                return false;
            }

            if (syntax.IsSelfClosing)
            {
                return false;
            }

            return syntax.HasChild(node => node.Name.LocalName == $"{platform.Grid.Name}.{platform.RowDefinitionsProperty}") == false
                    && syntax.HasChild(node => node.Name.LocalName == $"{platform.Grid.Name}.{platform.ColumnDefinitionsProperty}") == false;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Insert row and column definitions").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var workUnits = new List<IWorkUnit>();

            var xmlns = context.Namespaces.ResolveNamespaceForSchema(context.Platform.SchemaUrl);

            var rowDefinitions = new XmlNode();
            rowDefinitions.Name = new XmlName(syntax.Name.Namespace, context.Platform.RowDefinitionsProperty);

            var rowSetter = RowDefinitionGenerator.GenerateSyntax(xmlns.Prefix);

            rowDefinitions.AddChildNode(rowSetter);

            workUnits.Add(new InsertXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                HostSyntax = syntax,
                Syntax = rowDefinitions,
            });

            var columnDefinitions = new XmlNode();
            columnDefinitions.Name = new XmlName(syntax.Name.Namespace, context.Platform.ColumnDefinitionsProperty);

            var columnSetter = ColumnDefinitionGenerator.GenerateSyntax(xmlns.Prefix);

            columnDefinitions.AddChildNode(columnSetter);

            workUnits.Add(new InsertXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                HostSyntax = syntax,
                Syntax = columnDefinitions,
            });

            return workUnits;
        }
    }
}
