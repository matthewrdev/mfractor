using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Maui.CodeGeneration.Resources;
using MFractor.Maui.Configuration;
using MFractor.Maui.Utilities;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using MFractor.Work;
using MFractor.Code.WorkUnits;

namespace MFractor.Maui.CodeActions.Refactor.StaticResources
{
    class MoveStaticResourceDefinitionToAppXaml : RefactorXamlCodeAction
    {
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.move_static_resource_definition_to_app_xaml";

        public override string Name => "Move Static Resource To App.xaml";

        public override string Documentation => "Moves the declared static resource into the App.xaml";

        [Import]
        public IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        public IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (!syntax.HasParent
                || XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                return false;
            }

            var parent = syntax.Parent;

            var parentSymbol = context.XamlSemanticModel.GetSymbol(parent);

            if (parentSymbol is ITypeSymbol typeSymbol)
            {
                if (!SymbolHelper.DerivesFrom(typeSymbol,context.Platform.ResourceDictionary.MetaType))
                {
                    return false;
                }
            }
            else if (parentSymbol is IPropertySymbol property)
            {
                if (!SymbolHelper.DerivesFrom(property.Type,context.Platform.ResourceDictionary.MetaType))
                {
                    return false;
                }
            }

            return true;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var appXaml = AppXamlConfiguration.ResolveAppXamlFile(context.Project, context.Platform);

            if (appXaml != null && appXaml.FilePath == document.FilePath)
            {
                return null;
            }

            return CreateSuggestion("Move to resource to App.xaml").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var workUnits = new List<IWorkUnit>
            {
                new DeleteXmlSyntaxWorkUnit()
                {
                    Syntaxes = new List<XmlSyntax>()
                    {
                        syntax,
                    },
                    FilePath = document.FilePath,
                },
            };

            var appXaml = AppXamlConfiguration.ResolveAppXamlFile(context.Project, context.Platform);

            if (appXaml == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            workUnits.AddRange(InsertResourceEntryGenerator.Generate(appXaml, syntax));

            return workUnits;
        }
    }
}
