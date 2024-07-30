using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Maui;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services.Expressions
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class DataBindingExpressionCompletionService : IXamlCompletionService
    {
        readonly Lazy<IDataBindingCompletionService> dataBindingCompletionService;
        public IDataBindingCompletionService DataBindingCompletionService => dataBindingCompletionService.Value;

        readonly Lazy<IXamlSymbolResolver> symbolResolver;
        public IXamlSymbolResolver SymbolResolver => symbolResolver.Value;

        readonly Lazy<IBindingContextResolver> bindingContextResolver;
        public IBindingContextResolver BindingContextResolver => bindingContextResolver.Value;

        public string AnalyticsEvent => "Data Binding Completion (Expression)";

        [ImportingConstructor]
        public DataBindingExpressionCompletionService(Lazy<IDataBindingCompletionService> dataBindingCompletionService,
                                                      Lazy<IXamlSymbolResolver> symbolResolver,
                                                      Lazy<IBindingContextResolver> bindingContextResolver)
        {
            this.dataBindingCompletionService = dataBindingCompletionService;
            this.symbolResolver = symbolResolver;
            this.bindingContextResolver = bindingContextResolver;
        }

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            return xamlExpression != null;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var completions = new List<ICompletionSuggestion>();

            var syntax = xamlExpression;
            var caretOffset = triggerLocation.Position;

            var child = syntax.PreceedingChild(caretOffset);
            var preceedingCharacter = textView.TextBuffer.CurrentSnapshot.GetText(caretOffset - 1, 1);

            if (child is NameSyntax nameSyntax)
            {
                ProvideNameSyntaxCompletions(context, textView, completions, preceedingCharacter, nameSyntax);
            }
            else if (child is MemberNameSyntax memberName)
            {
                //ProvideMemberNameCompletions(completions, context, rr, expression, expressionSpan, caretOffset, memberName);
            }
            else if (child is ContentSyntax contentSyntax && syntax.PreceedingChild(contentSyntax.Span.Start) is NameSyntax name)
            {
                name = GetFullSymbolForNameSyntax(name);

                var contentCompletions = ProvideContentAndPropertyCompletions(context, textView, name);

                if (contentCompletions != null && contentCompletions.Any())
                {
                    completions.AddRange(contentCompletions);
                }
            }

            return completions;
        }

        NameSyntax ProvideNameSyntaxCompletions(IXamlFeatureContext featureContext, ITextView textView, List<ICompletionSuggestion> completions, string preceedingCharacter, NameSyntax nameSyntax)
        {
            nameSyntax = GetFullSymbolForNameSyntax(nameSyntax);
            if (string.IsNullOrWhiteSpace(preceedingCharacter))
            {
                var contentCompletions = ProvideContentAndPropertyCompletions(featureContext, textView, nameSyntax);

                if (contentCompletions != null && contentCompletions.Any())
                {
                    completions.AddRange(contentCompletions);
                }
            }
            else
            {
                //await ProvideNameCompletions(completions, context, rr, expression, expressionSpan, caretOffset, nameSyntax);
            }

            return nameSyntax;
        }

        NameSyntax GetEncapsulatingMarkupExtensionName(XamlExpressionSyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
            {
                return null;
            }

            if (syntaxNode is ExpressionSyntax expression)
            {
                return expression.NameSyntax;
            }

            return GetEncapsulatingMarkupExtensionName(syntaxNode.Parent);
        }

        NameSyntax GetFullSymbolForNameSyntax(NameSyntax nameSyntax)
        {
            if (nameSyntax == null)
            {
                return null;
            }

            if (nameSyntax is SymbolSyntax)
            {
                return nameSyntax;
            }

            if (nameSyntax is TypeNameSyntax typeName)
            {
                if (nameSyntax.Parent is SymbolSyntax)
                {
                    return typeName.Parent as SymbolSyntax;
                }

                return typeName;
            }

            if (nameSyntax is NamespaceSyntax @namespace)
            {
                if (nameSyntax.Parent is SymbolSyntax)
                {
                    return @namespace.Parent as SymbolSyntax;
                }

                return @namespace;
            }

            return null;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideContentAndPropertyCompletions(IXamlFeatureContext context, ITextView textView, NameSyntax nameSyntax)
        {
            if (nameSyntax == null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var platform = context.Platform;

            var xmlns = context.Namespaces.ResolveNamespace(nameSyntax);

            if (XamlSchemaHelper.IsMicrosoftSchema(xmlns))
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var extension = SymbolResolver.ResolveMarkupExtension(context.Project, context.Namespaces, context.XmlnsDefinitions, nameSyntax);

            if (extension == null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            if (SymbolHelper.DerivesFrom(extension, platform.BindingExtension.MetaType))
            {
                var bindingContext = BindingContextResolver.ResolveBindingContext(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, context.GetSyntax<XmlAttribute>()) as INamedTypeSymbol;

                if (bindingContext != null)
                {
                    return DataBindingCompletionService.ProvideBindingContextCompletions(context, bindingContext, false);
                }
            }

            return Array.Empty<ICompletionSuggestion>();
        }

    }
}
