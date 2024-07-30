using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.CodeGeneration;
using MFractor.Maui.CodeGeneration.Grids;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.Grids
{
    [CommonCodeAction]
    class InsertRowAndUpdate : RefactorXamlCodeAction
    {
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.insert_row_and_update";

        public override string Name => "Insert Grid Row And Update Elemets";

        public override string Documentation => "Inserts a row before or after the current row definition and adjust the indices and spans of all elements in this grid accordingly.";
        
        [Import]
        public IInsertGridRowColumnGenerator InsertGridRowColumnGenerator { get; set; }

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (symbol == null || symbol.ToString() != context.Platform.RowDefinition.MetaType)
            {
                return false;
            }

            return true;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return new List<ICodeActionSuggestion>()
            {
                CreateSuggestion($"Insert {context.Platform.RowProperty} Before", InsertionLocation.Start),
                CreateSuggestion($"Insert {context.Platform.RowProperty} After", InsertionLocation.End)
            };
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var definitions = syntax.Parent.GetChildren(c => c.Name.FullName == context.Platform.RowDefinition.Name);

            return InsertGridRowColumnGenerator.InsertGridRow(context.Platform,
                                                              syntax.Parent.Parent,
                                                              definitions.IndexOf(syntax),
                                                              suggestion.GetAction<InsertionLocation>(),
                                                              document.FilePath,
                                                              context.Platform.GridNamedSize_Auto);
        }
    }
}
