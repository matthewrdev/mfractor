using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeActions.Fix;
using MFractor.Maui.Symbols;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeActions.Generate
{
    class ImplementMissingMembers : GenerateXamlCodeAction
    {
        [ImportingConstructor]
        public ImplementMissingMembers(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        public override string Documentation => "Use the Generate Bindable Properties code action to create new bindable properties; it'll even figure out the property types for you!";

        public override string Identifier => "com.mfractor.code_actions.xaml.implement_missing_members";

        public override string Name => "Implement Missing Members";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        protected enum MissingMemberPropertyType
        {
            GetterSetter,
            BindableProperty,
        }

        [Import]
        public IEventHandlerDeclarationGenerator EventHandlerGenerator { get; set; }

        [Import]
        public IGeneratePropertyIntoParentTypeFix GeneratePropertyFix { get; set; }

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (!syntax.HasAttributes)
            {
                return false;
            }

            var elementSymbol = context.XamlSemanticModel.GetSymbol(syntax) as ITypeSymbol;
            if (elementSymbol == null
                || elementSymbol.DeclaringSyntaxReferences.Length == 0)
            {
                return false;
            }

            foreach (var attr in syntax.Attributes)
            {
                if (syntax.IsRoot
                    && attr.Name.HasNamespace ? attr.Name.Namespace == "xmlns" : attr.Name.LocalName == "xmlns")
                {
                    continue;
                }

                var ns = context.Namespaces.ResolveNamespace(attr);
                if (XamlSchemaHelper.IsMicrosoftSchema(ns))
                {
                    continue;
                }

                var symbol = context.XamlSemanticModel.GetSymbol(attr) as ISymbol;
                if (symbol == null)
                {
                    return true;
                }
            }

            return false;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var suggestions = new List<ICodeActionSuggestion>
            {
                CreateSuggestion("Implement missing members", MissingMemberPropertyType.GetterSetter)
            };

            var elementSymbol = context.XamlSemanticModel.GetSymbol(syntax) as ITypeSymbol;
            if (SymbolHelper.DerivesFrom(elementSymbol, context.Platform.BindableObject.MetaType))
            {
                suggestions.Add(CreateSuggestion("Implement missing members using bindable properties", MissingMemberPropertyType.BindableProperty));
            }

            return suggestions;
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var missingAttrs = new List<XmlAttribute>();
            foreach (var attr in syntax.Attributes)
            {
                if (syntax.IsRoot
                    && attr.Name.HasNamespace ? attr.Name.Namespace == "xmlns" : attr.Name.LocalName == "xmlns")
                {
                    continue;
                }

                var ns = context.Namespaces.ResolveNamespace(attr);
                if (XamlSchemaHelper.IsMicrosoftSchema(ns))
                {
                    continue;
                }

                if (context.XamlSemanticModel.GetSymbol(attr) as ISymbol == null)
                {
                    missingAttrs.Add(attr);
                }
            }

            var elementSymbol = context.XamlSemanticModel.GetSymbol(syntax) as ITypeSymbol;

            var propertyFixType = (MissingMemberPropertyType)suggestion.ActionId;

            var objectType = context.Compilation.GetTypeByMetadataName("System.Object");
            var eventArgsType = context.Compilation.GetTypeByMetadataName("System.EventArgs");

            var nodes = new List<SyntaxNode>();

            foreach (var attr in missingAttrs)
            {
                ISymbol resolvedType = objectType;
                var expression = context.XamlSemanticModel.GetExpression(attr);
                if (expression != null)
                {
                    var expressionResult = ExpressionEvaluater.Evaluate(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, attr);
                    if (expressionResult != null)
                    {
                        switch (expressionResult.SymbolKind)
                        {
                            case XamlSymbolKind.Symbol:
                                {
                                    var symbol = expressionResult.Symbol as ISymbol;
                                    if (symbol != null)
                                    {
                                        if (symbol is IFieldSymbol || symbol is IPropertySymbol)
                                        {
                                            resolvedType = SymbolHelper.ResolveMemberReturnType(symbol);
                                        }
                                        else if (symbol is IMethodSymbol) // Does this look like an event callback?
                                        {
                                            var methodSymbol = symbol as IMethodSymbol;
                                            if (methodSymbol.Parameters.Length == 2
                                                && SymbolHelper.DerivesFrom(methodSymbol.Parameters[1].Type, eventArgsType))
                                            {
                                                resolvedType = symbol;
                                            }
                                        }
                                    }
                                }
                                break;
                            case XamlSymbolKind.Image:
                                resolvedType = context.Compilation.GetTypeByMetadataName(context.Platform.ImageSource.MetaType);
                                break;
                            case XamlSymbolKind.Syntax:
                                break;
                        }
                    }
                }

                if (resolvedType is IMethodSymbol)
                {
                    var methodSymbol = resolvedType as IMethodSymbol;
                    nodes.AddRange(EventHandlerGenerator.GenerateSyntax(attr.Name.LocalName, "EventHandler", methodSymbol.Parameters[1].Type.ToString()));
                }
                else if (resolvedType is ITypeSymbol)
                {
                    var typeSymbol = resolvedType as ITypeSymbol;

                    if (propertyFixType == MissingMemberPropertyType.GetterSetter)
                    {
                        nodes.AddRange(GeneratePropertyFix.GeneratePropertySyntax(attr, context));
                    }
                    else if (propertyFixType == MissingMemberPropertyType.BindableProperty)
                    {
                        nodes.AddRange(GeneratePropertyFix.GenerateBindablePropertySyntax(attr, context));
                    }
                }
            }

            ClassDeclarationSyntax classSyntax = null;

            var generationTargetFile = "";
            var declaringSyntax = elementSymbol.DeclaringSyntaxReferences.FirstOrDefault(s => s.SyntaxTree.FilePath.EndsWith(".cs") && !s.SyntaxTree.FilePath.EndsWith(".g.cs"));

            generationTargetFile = declaringSyntax.SyntaxTree.FilePath;
            classSyntax = declaringSyntax.GetSyntax() as ClassDeclarationSyntax;

            var targetProject = SymbolHelper.GetProjectForSymbol(context.Solution, elementSymbol);

            return new InsertSyntaxNodesWorkUnit
            {
                HostNode = classSyntax,
                SyntaxNodes = nodes,
                Workspace = context.Workspace,
                Project = targetProject
            }.AsList();
        }

    }
}
