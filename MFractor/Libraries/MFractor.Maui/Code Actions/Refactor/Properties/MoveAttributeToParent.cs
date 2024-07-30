using System.Collections.Generic;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor.Properties
{
    class MoveAttributeToParent : RefactorXamlCodeAction
    {
        public override string Documentation => "Moves an attribute into it's parent element";

        public override string Identifier => "com.mfractor.code_actions.xaml.move_attribute_to_parent";

        public override string Name => "Move Attribute To Parent";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (XamlSyntaxHelper.ExplodeAttachedProperty(syntax.Name.FullName, out _, out _))
            {
                return false;
            }

            var parentAttribute = syntax.Parent?.Parent?.GetAttributeByName(syntax.Name.FullName);
            if (parentAttribute != null)
            {
                return false;
            }

            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;

            if (property == null)
            {
                return false;
            }

            var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent.Parent) as INamedTypeSymbol;

            var parentProperty = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(parentSymbol, syntax.Name.FullName);

            if (parentProperty == null)
            {
                return false;
            }

            return property.Name == parentProperty.Name && property.Type.ToString() == parentProperty.Type.ToString();
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Move {syntax.Name.FullName} to parent element").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return new List<IWorkUnit>()
            {
                new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntaxes = new List<XmlSyntax>() { syntax },
                },
                new InsertXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    HostSyntax = syntax.Parent.Parent,
                    Syntax = syntax,
                }
            };
        }
    }
}