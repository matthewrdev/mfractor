using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.CodeGeneration.Grids;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.Grids
{
    class ConvertStackLayoutToGrid : RefactorXamlCodeAction
    {
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.convert_stacklayout_to_grid";

        public override string Name => "Convert StackLayout To Grid";

        public override string Documentation => "Given a StackLayout in XAML, this code action converts it to a vertically or horizontally aligned grid.";

        [Import]
        public IGridColumnDefinitionGenerator ColumnDefinitionGenerator { get; set; }

        [Import]
        public IGridRowDefinitionGenerator RowDefinitionGenerator { get; set; }

        [Import]
        public IRenameXmlNodeGenerator RenameXmlNodeGenerator { get; set; }

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (symbol == null)
            {
                return false;
            }

            var isSupported = symbol.ToString() == context.Platform.StackLayout.MetaType;
            if (context.Platform.SupportsTypedOrientationStackLayouts
                && !isSupported)
            {
                isSupported = symbol.ToString() == context.Platform.VerticalStackLayout.MetaType
                              || symbol.ToString() == context.Platform.HorizontalStackLayout.MetaType;
            }

            return isSupported;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            return CreateSuggestion($"Convert {symbol.Name} to {context.Platform.Grid.Name}").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            var orientation = syntax.GetAttributeByName(context.Platform.OrientationProperty)?.Value?.Value ?? context.Platform.StackLayoutOrientation_Vertical;

            if (orientation == context.Platform.StackLayoutOrientation_Vertical
                || symbol.ToString() == context.Platform.VerticalStackLayout.MetaType)
            {
                return ConvertToVerticalGrid(syntax, document, context);
            }

            if (orientation == context.Platform.StackLayoutOrientation_Horizontal
                || symbol.ToString() == context.Platform.HorizontalStackLayout.MetaType)
            {
                return ConvertToHorizontalGrid(syntax, document, context);
            }

            return null;
        }

        bool IsExpanded(XmlNode node, string optionsName)
        {
            var options = node.GetAttributeByName(optionsName);

            if (options == null || !options.HasValue)
            {
                return false;
            }

            var layoutOptionsValue = options.Value.Value;

            if (string.IsNullOrEmpty(layoutOptionsValue))
            {
                return false;
            }

            return layoutOptionsValue.EndsWith("Expand", System.StringComparison.OrdinalIgnoreCase);
        }

        IReadOnlyList<IWorkUnit> ConvertToHorizontalGrid(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var platform = context.Platform;
            var columnDefinitionPropertyName = $"{context.Platform.Grid.Name}.{context.Platform.ColumnDefinitionsProperty}";

            var workUnits = new List<IWorkUnit>();
            var xmlns = context.Namespaces.ResolveNamespaceForSchema(platform.SchemaUrl);

            var grid = context.Compilation.GetTypeByMetadataName(context.Platform.Grid.MetaType);

            workUnits.AddRange(RenameXmlNodeGenerator.Rename(syntax, grid, document.Namespaces.DefaultNamespace.Prefix, syntax.Name.FullName, document, context.XamlSemanticModel, context.Platform));

            var visualElements = syntax.GetChildren((c) =>
           {
               if (XamlSyntaxHelper.IsPropertySetter(c))
               {
                   return false;
               }

               var symbol = context.XamlSemanticModel.GetSymbol(c) as INamedTypeSymbol;

               return SymbolHelper.DerivesFrom(symbol, context.Platform.VisualElement.MetaType);
           });


            var columnDefinitions = new XmlNode
            {
                Name = new XmlName(syntax.Name.Namespace, columnDefinitionPropertyName)
            };

            if (visualElements.Any())
            {
                foreach (var element in visualElements)
                {
                    var idx = visualElements.IndexOf(element);

                    var attr = $" {platform.Grid.Name}.{platform.ColumnProperty}=\"{idx}\" ";

                    workUnits.Add(new InsertTextWorkUnit(attr, element.NameSpan.End, document.FilePath));

                    var isExpanded = IsExpanded(element, "HorizontalOptions");
                    var columnSetter = ColumnDefinitionGenerator.GenerateSyntax(xmlns.Prefix, isExpanded ? context.Platform.GridNamedSize_Star : context.Platform.GridNamedSize_Auto);

                    columnDefinitions.AddChildNode(columnSetter);
                }
            }

            var orientation = syntax.GetAttributeByName(context.Platform.OrientationProperty);
            if (orientation != null)
            {
                workUnits.Add(new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntaxes = new List<XmlSyntax>()
                    {
                        orientation
                    }
                });
            }

            workUnits.Add(new InsertXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                HostSyntax = syntax,
                Syntax = columnDefinitions,
            });

            return workUnits;
        }

        IReadOnlyList<IWorkUnit> ConvertToVerticalGrid(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var platform = context.Platform;
            var rowDefinitionPropertyName = $"{context.Platform.Grid.Name}.{context.Platform.RowDefinitionsProperty}";

            var workUnits = new List<IWorkUnit>();

            var grid = context.Compilation.GetTypeByMetadataName(context.Platform.Grid.MetaType);

            workUnits.AddRange(RenameXmlNodeGenerator.Rename(syntax, grid, document.Namespaces.DefaultNamespace.Prefix, document, context.Project, context.XamlSemanticModel, context.Platform));

            var visualElements = syntax.GetChildren((c) =>
            {
                if (XamlSyntaxHelper.IsPropertySetter(c))
                {
                    return false;
                }

                var symbol = context.XamlSemanticModel.GetSymbol(c) as INamedTypeSymbol;

                return SymbolHelper.DerivesFrom(symbol, context.Platform.VisualElement.MetaType);
            });

            var xmlns = context.Namespaces.ResolveNamespaceForSchema(platform.SchemaUrl);

            var rowDefinitions = new XmlNode
            {
                Name = new XmlName(syntax.Name.Namespace, rowDefinitionPropertyName)
            };

            if (visualElements.Any())
            {
                foreach (var element in visualElements)
                {
                    var idx = visualElements.IndexOf(element);

                    var attr = $" {platform.Grid.Name}.{platform.RowProperty}=\"{idx}\" ";

                    workUnits.Add(new InsertTextWorkUnit(attr, element.NameSpan.End, document.FilePath));

                    var isExpanded = IsExpanded(element, "VerticalOptions");
                    var rowSetter = RowDefinitionGenerator.GenerateSyntax(xmlns.Prefix, isExpanded ? context.Platform.GridNamedSize_Star : context.Platform.GridNamedSize_Auto);

                    rowDefinitions.AddChildNode(rowSetter);
                }
            }

            var orientation = syntax.GetAttributeByName(platform.OrientationProperty);
            if (orientation != null)
            {
                workUnits.Add(new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntaxes = new List<XmlSyntax>()
                    {
                        orientation
                    }
                });
            }

            workUnits.Add(new InsertXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                HostSyntax = syntax,
                Syntax = rowDefinitions,
            });

            return workUnits;
        }
    }
}
