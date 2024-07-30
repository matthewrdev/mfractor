using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Maui.CodeGeneration.Grids;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.Grids
{
    [CommonCodeAction]
    class DeleteColumnAndUpdate : RefactorXamlCodeAction
    {
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.delete_column_and_update";

        public override string Name => "Delete Grid Column And Update Children";

        public override string Documentation => "Deletes grids column and adjust the indices and spans of all elements in this grid accordingly.";
        
        [Import]
        public IDeleteGridRowColumnGenerator DeleteGridRowColumnGenerator { get; set; }

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (symbol == null || symbol.ToString() != context.Platform.ColumnDefinition.MetaType)
            {
                return false;
            }

            return true;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Delete {context.Platform.ColumnProperty}").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var definitions = syntax.Parent.GetChildren(c => c.Name.FullName == context.Platform.ColumnDefinition.Name);

            return DeleteGridRowColumnGenerator.DeleteGridColumn(context.Platform, syntax.Parent.Parent, definitions.IndexOf(syntax), document.FilePath);
        }
    }
}
