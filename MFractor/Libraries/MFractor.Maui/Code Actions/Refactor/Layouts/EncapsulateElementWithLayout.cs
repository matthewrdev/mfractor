using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Attributes;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.Layouts
{
    class EncapsulateWithLayout : RefactorXamlCodeAction
    {
        [ImportingConstructor]
        public EncapsulateWithLayout(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        public override string Documentation => "This refactoring allows the user to wrap a view within a Layout such as a Grid, StackLayout, ContentView, etc";

        public override string Identifier => "com.mfractor.code_actions.xaml.encapsulate_with_layout";

        public override string Name => "Encapsulate With Layout";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            return SymbolHelper.DerivesFrom(typeSymbol, context.Platform.View.MetaType);
        }

        readonly IReadOnlyList<string> layouts = new List<string>()
        {
            "StackLayout",
            "Grid",
            "FlexLayout",
            "Frame",
            "ContentView",
            "AbsoluteLayout",
            "RelativeLayout",
            "ScrollView",
        };

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Encapsulate with layout").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax,
                                                       IParsedXamlDocument document,
                                                       IXamlFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            var choices = layouts.ToDictionary(l => l, l => (object)l);
            return new PickerWorkUnit()
            {
                Title = "Encapsulate With Layout",
                Message = $"Choose a layout to encapsulate {syntax.Name.FullName} within.",
                Choices = choices,
                Delegate = (choice) =>
                {
                    WorkEngine.ApplyAsync(new EncapsulateXmlSyntaxWorkUnit()
                    {
                        FilePath = document.FilePath,
                        NewParent = new XmlNode(choice as string),
                        Target = syntax,
                    });
                }
            }.AsList();
        }
    }
}