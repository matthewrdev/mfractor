using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.TypeInferment;
using MFractor.Code.WorkUnits;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.Analysis;
using MFractor.Maui.CodeGeneration.BindableProperties;
using MFractor.Maui.CodeGeneration.CSharp;
using MFractor.Maui.Configuration;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeActions.Fix.Attributes
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IGeneratePropertyIntoParentTypeFix))]
    class CreateMissingPropertyForClassFix : FixCodeAction, IGeneratePropertyIntoParentTypeFix
    {
        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        readonly Lazy<ITypeInfermentService> typeInfermentService;
        public ITypeInfermentService TypeInfermentService => typeInfermentService.Value;

        public override Type TargetCodeAnalyser => typeof(PropertySetterAttributeDoesNotExistInParentAnalysis);

        public override string Documentation => "When a XAML element references a member that does not exist, this fix can generate that property/field/method/event onto the class";

        public override string Identifier => "com.mfractor.code_fixes.xaml.generate_property";

        public override string Name => "Create Missing Property For Class";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public IValueConverterTypeInfermentConfiguration TypeInfermentConfiguration { get; set; }

        [Import]
        public IBindablePropertyGenerator BindablePropertyGenerator { get; set; }

        [Import]
        public IViewModelPropertyGenerator PropertyGenerator { get; set; }

        [Import]
        public IEventHandlerDeclarationGenerator EventHandlerGenerator { get; set; }

        [ImportingConstructor]
        public CreateMissingPropertyForClassFix(Lazy<IMarkupExpressionEvaluater> expressionEvaluater,
                                                Lazy<ITypeInfermentService> typeInfermentService)
        {
            this.expressionEvaluater = expressionEvaluater;
            this.typeInfermentService = typeInfermentService;
        }

        protected override bool CanExecute(ICodeIssue issue,
                                           XmlAttribute syntax,
                                           IParsedXamlDocument document,
                                           IXamlFeatureContext context,
                                           InteractionLocation location)
        {
            var parentNode = syntax.Parent;
            var elementSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent) as ITypeSymbol;
            if (elementSymbol == null)
            {
                return false;
            }

            var xamlNamespace = context.Namespaces.ResolveNamespace(parentNode);
            if (xamlNamespace == null)
            {
                return false;
            }

            return elementSymbol.DeclaringSyntaxReferences.Length > 0;
        }

        protected enum GeneratePropertyType
        {
            Property,
            BindableProperty,
            EventHandler,
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue,
                                                                     XmlAttribute syntax,
                                                                     IParsedXamlDocument document,
                                                                     IXamlFeatureContext context,
                                                                     InteractionLocation location)
        {
            var suggestions = new List<ICodeActionSuggestion>();

            var elementSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent) as ITypeSymbol;

            if (SymbolHelper.DerivesFrom(elementSymbol, context.Platform.BindableObject.MetaType))
            {
                suggestions.Add(CreateSuggestion($"Generate a bindable property named {syntax.Name.LocalName} into {elementSymbol.Name}", GeneratePropertyType.BindableProperty));
            }

            if (syntax.Name.LocalName.EndsWith("callback", StringComparison.OrdinalIgnoreCase)
                || syntax.Name.LocalName.EndsWith("event", StringComparison.OrdinalIgnoreCase)
                || syntax.Name.LocalName.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add(CreateSuggestion($"Generate an event handler named {syntax.Name.LocalName} into {elementSymbol.Name}", GeneratePropertyType.EventHandler));
            }

            suggestions.Add(CreateSuggestion($"Generate a property named {syntax.Name.LocalName} into {elementSymbol.Name}", GeneratePropertyType.Property));

            return suggestions;

        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue,
                                                           XmlAttribute syntax,
                                                          IParsedXamlDocument document,
                                                           IXamlFeatureContext context,
                                                           ICodeActionSuggestion suggestion,
                                                           InteractionLocation location)
        {
            if (!TryGetClassSyntax(syntax, context, out var targetSymbol, out var classSyntax))
            {
                return null;
            }

            List<MemberDeclarationSyntax> code;
            if (suggestion.IsAction(GeneratePropertyType.BindableProperty))
            {
                code = GenerateBindablePropertySyntax(syntax, context).ToList();
            }
            else if (suggestion.ActionId == (int)GeneratePropertyType.EventHandler)
            {
                code = EventHandlerGenerator.GenerateSyntax(syntax.Name.LocalName, "EventHandler<System.EventArgs>").ToList();
            }
            else
            {
                code = GeneratePropertySyntax(syntax, context).ToList();
            }

            if (code == null)
            {
                return null;
            }

            var targetProject = SymbolHelper.GetProjectForSymbol(context.Solution, targetSymbol);

            return new InsertSyntaxNodesWorkUnit()
            {
                HostNode = classSyntax,
                SyntaxNodes = code,
                Workspace = context.Workspace,
                Project = targetProject
            }.AsList();
        }

        public IEnumerable<MemberDeclarationSyntax> GeneratePropertySyntax(XmlAttribute element, IXamlFeatureContext context)
        {
            var parentNode = element.Parent;

            var elementSymbol = context.XamlSemanticModel.GetSymbol(parentNode) as ISymbol;
            if (elementSymbol == null)
            {
                return null;
            }

            var propertyType = context.Compilation.GetTypeByMetadataName("System.Object");
            var expression = context.XamlSemanticModel.GetExpression(element);
            if (expression != null)
            {
                var symbolResult = ExpressionEvaluater.Evaluate(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, expression);
                if (symbolResult != null && symbolResult.Symbol is ISymbol)
                {
                    var returnedType = SymbolHelper.ResolveMemberReturnType(symbolResult.Symbol as ISymbol) as INamedTypeSymbol;
                    if (returnedType != null)
                    {
                        propertyType = returnedType;
                    }
                }
            }

            var code = PropertyGenerator.GenerateSyntax(propertyType, Accessibility.Public, element.Name.LocalName, null);

            return code;
        }

        public bool TryGetClassSyntax(XmlAttribute element, IXamlFeatureContext context, out INamedTypeSymbol targetSymbol, out ClassDeclarationSyntax classSyntax)
        {
            classSyntax = null;

            var parentNode = element.Parent;
            targetSymbol = context.XamlSemanticModel.GetSymbol(parentNode) as INamedTypeSymbol;
            if (targetSymbol == null)
            {
                return false;
            }

            var declaringSyntax = targetSymbol.DeclaringSyntaxReferences.GetNonAutogeneratedSyntax();

            classSyntax = declaringSyntax as ClassDeclarationSyntax;

            return true;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateBindablePropertySyntax(XmlAttribute syntax, IXamlFeatureContext context)
        {
            var parentNode = syntax.Parent;

            var elementSymbol = context.XamlSemanticModel.GetSymbol(parentNode) as ITypeSymbol;
            if (elementSymbol == null)
            {
                return null;
            }

            var knowsType = false;
            var propertyType = TypeInfermentConfiguration.DefaultType;

            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                var symbolResult = ExpressionEvaluater.Evaluate(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, expression);
                if (symbolResult != null && symbolResult.Symbol is ISymbol)
                {
                    var returnedType = SymbolHelper.ResolveMemberReturnType(symbolResult.Symbol as ISymbol);
                    if (returnedType != null)
                    {
                        propertyType = returnedType.ToString();
                        knowsType = true;
                    }
                }
            }

            if (!knowsType
                && TypeInfermentConfiguration.TryInferUnknownTypes)
            {
                propertyType = TypeInfermentService.InferTypeFromNameAndValue(syntax.Name.LocalName,
                                                                             syntax.Value?.Value,
                                                                             context.Platform.Color.MetaType,
                                                                             context.Platform.ImageSource.MetaType,
                                                                             TypeInfermentConfiguration.DefaultType,
                                                                             context.Compilation);

                if (expression != null && propertyType == "string")
                {
                    propertyType = TypeInfermentConfiguration.DefaultType;
                }
            }

            var elementType = elementSymbol.ToString();

            var code = BindablePropertyGenerator.GenerateSyntax(context.Platform, syntax.Name.LocalName, propertyType, elementType, BindablePropertyKind.Default);

            return code;
        }
    }
}
