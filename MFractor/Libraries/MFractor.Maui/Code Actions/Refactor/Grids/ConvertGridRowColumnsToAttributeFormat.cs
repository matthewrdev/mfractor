using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.CodeActions;
using MFractor.Maui.Grids;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.Grids
{
    class ConvertGridRowColumnsToAttributeFormat : RefactorXamlCodeAction
    {
        readonly Lazy<IGridAxisResolver> gridAxisResolver;
        public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.convert_grid_row_columns_to_attribute_format";

        public override string Name => "Convert Grid Row/Columns To Attribute Format";

        public override string Documentation => "When a grids rows or columns are declared using the node format, this refactoring lets you convert into the shortened attribute row/column definition format.";

        [ImportingConstructor]
        public ConvertGridRowColumnsToAttributeFormat(Lazy<IGridAxisResolver> gridAxisResolver)
        {
            this.gridAxisResolver = gridAxisResolver;
        }

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (!XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                return false;
            }

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
                   || SymbolHelper.DerivesFrom(property.Type, context.Platform.RowDefinitionCollection.MetaType);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            XamlSyntaxHelper.ExplodePropertySetter(syntax, out _, out var propertyName);

            return CreateSuggestion($"Convert to {propertyName} attribute format").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            XamlSyntaxHelper.ExplodePropertySetter(syntax, out _, out var propertyName);

            var result = new List<IWorkUnit>()
            {
                new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntax = syntax,
                }
            };

            var children = new List<XmlNode>();

            List<IGridAxisDefinition> elements = default;

            if (propertyName == context.Platform.RowDefinitionsProperty)
            {
                elements = GridAxisResolver.ResolveRowDefinitions(syntax.Parent, context.Platform).ToList();
            }
            else if (propertyName == context.Platform.ColumnDefinitionsProperty)
            {
                elements = GridAxisResolver.ResolveColumnDefinitions(syntax.Parent, context.Platform).ToList();
            }

            result.Add(new InsertXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                HostSyntax = syntax.Parent,
                Syntax = new XmlAttribute(propertyName, string.Join(", ", elements.Select(e => e.Value.Trim())))
            });

            return result;
        }
    }
}