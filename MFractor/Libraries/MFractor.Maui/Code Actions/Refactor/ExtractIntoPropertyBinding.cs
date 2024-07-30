using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.CodeGeneration.CSharp;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor
{
    class ExtractIntoPropertyBinding : RefactorXamlCodeAction
    {
        public override string Documentation => "Extracts the property value and converts it into a new property on the view model";

        public override string Identifier => "com.mfractor.code_actions.xaml.extract_into_property_binding";

        public override string Name => "Extract Attribute Value Into Property Binding";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public IViewModelPropertyGenerator PropertyGenerator { get; set; }

        public override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return false;
            }

            var bindingContext = document.BindingContext;
            if (bindingContext == null)
            {
                return false;
            }

            var propertySymbol = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
            if (propertySymbol == null)
            {
                return false;
            }

            // Does this property map to a bindable property?
            var propertyName = syntax.Name.LocalName;
            if (XamlSyntaxHelper.IsAttachedProperty(propertyName))
            {
                XamlSyntaxHelper.ExplodeAttachedProperty(propertyName, out _, out propertyName);
            }

            propertyName += "Property";
            var attachedPropertySymbol = propertySymbol.ContainingType.GetMembers(propertyName);
            if (!attachedPropertySymbol.Any())
            {
                return false;
            }

            return true;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Extract Value Into Property Binding", 0).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax,
                                                       IParsedXamlDocument document,
                                                       IXamlFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            IReadOnlyList<IWorkUnit> generateBindingExpression(string userInput)
            {
                var workUnits = new List<IWorkUnit>();

                var propertySymbol = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
                var propertyType = SymbolHelper.ResolveMemberReturnType(propertySymbol);

                var declaringSyntax = document.BindingContext.DeclaringSyntaxReferences.First();

                var classSyntax = declaringSyntax.GetSyntax() as Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;

                var nodes = PropertyGenerator.GenerateSyntax(propertyType, Accessibility.Public, userInput, syntax.Value?.Value);

                var targetProject = SymbolHelper.GetProjectForSymbol(context.Solution, document.BindingContext);

                workUnits.Add(new InsertSyntaxNodesWorkUnit
                {
                    HostNode = classSyntax,
                    SyntaxNodes = nodes.ToList(),
                    Workspace = context.Workspace,
                    Project = targetProject
                });

                var xmlns = context.Namespaces.ResolveNamespaceForSchema(context.Platform.SchemaUrl);
                var bindingLiteral = "Binding";
                if (!string.IsNullOrEmpty(xmlns.Prefix))
                {
                    bindingLiteral = xmlns.Prefix + ":Binding";
                }

                var bindingExpression = "{" + bindingLiteral + " " + userInput + "}";

                workUnits.Add(new ReplaceTextWorkUnit(document.FilePath, bindingExpression, syntax.Value.Span));

                return workUnits;
            }

            return new TextInputWorkUnit("Extract Value Into Property Binding",
                                               "Enter the name for the property binding...",
                                               "",
                                               "Extract",
                                               "Cancel",
                                               generateBindingExpression).AsList();
        }
    }
}
