using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.Layouts
{
    class ChangeLayoutKind : RefactorXamlCodeAction
    {
        public override string Documentation => "This refactoring allows the user to the root layout between commonly used layout kinds such as ContentView, Grid, Frame, StackLayout or FlexLayout.";

        public override string Identifier => "com.mfractor.code_actions.xaml.change_layout_kind";

        public override string Name => "Change Layout";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
        public IRenameXmlNodeGenerator RenameXmlNodeGenerator { get; set; }

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if(!syntax.IsRoot)
            {
                return false;
            }

            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            var layoutMappings = GetMappings(context.Platform);

            return layoutMappings.Any(mapping => SymbolHelper.DerivesFrom(typeSymbol, mapping.Value.MetaType));
        }

        enum LayoutKind
        {
            StackLayout,

            Grid,

            FlexLayout,

            Frame,

            ContentView,

            AbsoluteLayout,

            RelativeLayout,

            ScrollView,
        }

        private IReadOnlyDictionary<LayoutKind, ITypeDefinition> GetMappings(IXamlPlatform platform)
        {
            return new Dictionary<LayoutKind, ITypeDefinition>()
            {
                { LayoutKind.StackLayout, platform.StackLayout},
                { LayoutKind.Grid, platform.Grid},
                { LayoutKind.FlexLayout, platform.FlexLayout},
                { LayoutKind.Frame, platform.Frame},
                { LayoutKind.ContentView, platform.ContentView},
                { LayoutKind.AbsoluteLayout, platform.AbsoluteLayout},
                { LayoutKind.RelativeLayout, platform.RelativeLayout},
                { LayoutKind.ScrollView,  platform.ScrollView},
            };
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            var layoutMappings = GetMappings(context.Platform);

            var suggestions = new List<ICodeActionSuggestion>();

            foreach (var conversion in layoutMappings)
            {
                var type = context.Compilation.GetTypeByMetadataName(conversion.Value.MetaType);

                if (SymbolHelper.DerivesFrom(typeSymbol, context.Platform.StackLayout.MetaType)
                    && SymbolHelper.DerivesFrom(type, context.Platform.Grid.MetaType))
                {
                    continue;
                }

                if (type != null)
                {
                    suggestions.Add(CreateSuggestion($"Convert to {conversion.Value.Name}", conversion.Key));
                }
            }

            return suggestions;
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax,
                                                       IParsedXamlDocument document,
                                                       IXamlFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {

            var layoutMappings = GetMappings(context.Platform);

            var layoutKind = suggestion.GetAction<LayoutKind>();

            var selection = layoutMappings[layoutKind];

            var namedType = context.Compilation.GetTypeByMetadataName(selection.MetaType);

            return RenameXmlNodeGenerator.Rename(syntax, namedType, syntax.Name.FullName, document, context.Project, context.XamlSemanticModel, context.Platform);
        }
    }
}
