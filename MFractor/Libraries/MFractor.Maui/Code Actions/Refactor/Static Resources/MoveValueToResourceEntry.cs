using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeActions;
using MFractor.Maui.CodeGeneration.Resources;
using MFractor.Maui.Configuration;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.StaticResources
{
    class MoveValueToResourceEntry : RefactorXamlCodeAction
    {
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_actions.xaml.move_value_to_resource_entry";

        public override string Name => "Move Value To Resource Entry";

        public override string Documentation => "Moves the attributes value to the resource dictionary in the current file or in the App.xaml.";

        [Import]
        public IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        public IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        public enum Target
        {
            Local,
            AppXaml,
        }

        public override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var appXaml = AppXamlConfiguration.ResolveAppXamlFile(context.Project, context.Platform);

            if (appXaml == null || appXaml.FilePath == document.FilePath)
            {
                return false;
            }

            if (!syntax.HasValue || ExpressionParserHelper.IsExpression(syntax.Value.Value))
            {
                return false;
            }

            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;

            if (property == null)
            {
                return false;
            }

            return SymbolHelper.DerivesFrom(property.Type, context.Platform.Color.MetaType)
                   || SymbolHelper.DerivesFrom(property.Type, context.Platform.Thickness.MetaType)
                   || property.Type.SpecialType == SpecialType.System_String;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;

            var kind = property.Type.Name.ToLower();

            return new List<ICodeActionSuggestion>()
            {
                CreateSuggestion($"Move {kind} to local resource", Target.Local),
                CreateSuggestion($"Move {kind} to App.xaml resource", Target.AppXaml),
            };
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;

            IReadOnlyList<IWorkUnit> onTextInputConfirmed(string value)
            {
                var workUnits = new List<IWorkUnit>()
                {
                    new ReplaceTextWorkUnit()
                    {
                        FilePath = document.FilePath,
                        Text = "{" + context.Platform.StaticResourceExtension.MarkupExpressionName + " " + value +"}",
                        Span = syntax.Value.Span,
                    }
                };

                var targetFile = suggestion.IsAction(Target.AppXaml) ? AppXamlConfiguration.ResolveAppXamlFile(context.Project, context.Platform) : document.ProjectFile;

                var typeName = property.Type.Name;
                if (property.Type.SpecialType == SpecialType.System_String)
                {
                    typeName = "x:String";
                }

                var entry = new XmlNode(typeName);
                entry.AddAttribute("x:Key", value);
                entry.Value = syntax.Value.Value;

                workUnits.AddRange(InsertResourceEntryGenerator.Generate(targetFile, entry));

                return workUnits;
            }

            return new TextInputWorkUnit("Extract Resource Value", "Enter the name of the new resource", string.Empty, "Extract", "Cancel", onTextInputConfirmed).AsList();
        }
    }
}