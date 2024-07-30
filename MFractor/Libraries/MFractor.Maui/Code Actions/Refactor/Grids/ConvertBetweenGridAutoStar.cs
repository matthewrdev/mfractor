using System.Collections.Generic;
using MFractor.Code.CodeActions;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.Grids
{
    class ConvertBetweenGridAutoStar : RefactorXamlCodeAction
    {
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.convert_between_grid_star_auto";

        public override string Name => "Convert Between Grid Star And Auto";

        public override string Documentation => "Given a grid row or column that uses the `*` or `Auto` settings, this code action converts stars to autos and auto to stars.";

        enum ConversionMode
        {
            ToStar,
            ToAuto,
        }

        public override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (syntax.Name.FullName != context.Platform.ColumnWidthProperty && syntax.Name.FullName != context.Platform.RowHeightProperty)
            {
                return false;
            }

            if (!syntax.Value.HasValue)
            {
                return false;
            }

            var type = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(type, context.Platform.RowDefinition.MetaType) && !SymbolHelper.DerivesFrom(type, context.Platform.ColumnDefinition.MetaType))
            {
                return false;
            }

            return syntax.Value.Value == context.Platform.GridNamedSize_Auto || syntax.Value.Value == context.Platform.GridNamedSize_Star;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (syntax.Value.Value == context.Platform.GridNamedSize_Auto)
            {
                return CreateSuggestion($"Convert to {context.Platform.GridNamedSize_Star}", ConversionMode.ToStar).AsList();
            }

            return CreateSuggestion($"Convert to {context.Platform.GridNamedSize_Auto}", ConversionMode.ToAuto).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var replacement = context.Platform.GridNamedSize_Auto;
            if (suggestion.IsAction(ConversionMode.ToStar))
            {
                replacement = context.Platform.GridNamedSize_Star;
            }

            return new ReplaceTextWorkUnit()
            {
                Text = replacement,
                FilePath = document.FilePath,
                Span = syntax.Value.Span,
            }.AsList();
        }
    }
}
