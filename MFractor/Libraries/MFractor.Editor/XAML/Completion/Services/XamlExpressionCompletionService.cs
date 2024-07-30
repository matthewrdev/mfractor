using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    class XamlExpressionCompletionService : IAsyncXamlCompletionService
    {
        readonly Lazy<IDynamicResourceCompletionService> dynamicResourceCompletionService;
        public IDynamicResourceCompletionService DynamicResourceCompletionService => dynamicResourceCompletionService.Value;

        readonly Lazy<IStaticResourceCompletionService> staticResourceCompletionService;
        public IStaticResourceCompletionService StaticResourceCompletionService => staticResourceCompletionService.Value;

        readonly Lazy<IXamlSymbolResolver> symbolResolver;
        public IXamlSymbolResolver SymbolResolver => symbolResolver.Value;

        public string AnalyticsEvent => "Expression Completion";

        [ImportingConstructor]
        public XamlExpressionCompletionService(Lazy<IDynamicResourceCompletionService> dynamicResourceCompletionService,
                                               Lazy<IStaticResourceCompletionService> staticResourceCompletionService,
                                               Lazy<IXamlSymbolResolver> symbolResolver)
        {
            this.dynamicResourceCompletionService = dynamicResourceCompletionService;
            this.staticResourceCompletionService = staticResourceCompletionService;
            this.symbolResolver = symbolResolver;
        }

        public Task<bool> CanProvideCompletionsAsync(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            return Task.FromResult(CanProvideCompletions(textView, context, xamlExpression, triggerLocation, applicableToSpan, token));
        }

        public async Task<IReadOnlyList<ICompletionSuggestion>> ProvideCompletionsAsync(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var completions = new List<ICompletionSuggestion>();

            var syntax = xamlExpression;
            var caretOffset = triggerLocation.Position;

            var child = syntax.PreceedingChild(caretOffset);
            var preceedingCharacter = textView.TextBuffer.CurrentSnapshot.GetText(caretOffset - 1, 1);

            if (child is NameSyntax nameSyntax)
            {
                await ProvideNameSyntaxCompletions(context, textView, completions, preceedingCharacter, nameSyntax);
            }
            else if (child is MemberNameSyntax memberName)
            {
                //ProvideMemberNameCompletions(completions, context, rr, expression, expressionSpan, caretOffset, memberName);
            }
            else if (child is ContentSyntax contentSyntax && syntax.PreceedingChild(contentSyntax.Span.Start) is NameSyntax name)
            {
                name = GetFullSymbolForNameSyntax(name);

                var contentCompletions = await ProvideContentAndPropertyCompletionsAsync(context, textView, name);

                if (contentCompletions != null && contentCompletions.Any())
                {
                    completions.AddRange(contentCompletions);
                }
            }
            else if (child is ElementSyntax elementSyntax)
            {
                //await ProvideElementCompletion(completions, context, rr, expression, expressionSpan, caretOffset, elementSyntax);
            }
            else if (child is ExpressionSyntax expressionSyntax)
            {
                //await ProvideExpressionCompletions(completions, context, rr, expression, expressionSpan, caretOffset, expressionSyntax);
            }
            else
            {
                //ProvideCompletions(completions, context, rr, expression, expressionSpan, caretOffset, child);
            }

            return completions;
        }

        async Task<NameSyntax> ProvideNameSyntaxCompletions(IXamlFeatureContext featureContext, ITextView textView, List<ICompletionSuggestion> completions, string preceedingCharacter, NameSyntax nameSyntax)
        {
            nameSyntax = GetFullSymbolForNameSyntax(nameSyntax);
            if (string.IsNullOrWhiteSpace(preceedingCharacter))
            {
                var contentCompletions = await ProvideContentAndPropertyCompletionsAsync(featureContext, textView, nameSyntax);

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

        public async Task<IReadOnlyList<ICompletionSuggestion>> ProvideContentAndPropertyCompletionsAsync(IXamlFeatureContext context, ITextView textView, NameSyntax nameSyntax)
        {
            if (nameSyntax == null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

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

            if (SymbolHelper.DerivesFrom(extension, context.Platform.StaticResourceExtension.MetaType))
            {
                var attribute = context.GetSyntax<XmlAttribute>();

                var property = context.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;

                return await StaticResourceCompletionService.ProvideStaticResourceCompletions(context, property, false);
            }
            else if (SymbolHelper.DerivesFrom(extension, context.Platform.DynamicResourceExtension.MetaType))
            {
                return DynamicResourceCompletionService.ProvideDynamicResourceCompletions(context, false);
            }
            else if (SymbolHelper.DerivesFrom(extension, context.Platform.BindingExtension.MetaType))
            {
                // Handled by sync implementation.
            }

            return Array.Empty<ICompletionSuggestion>();
        }

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            return xamlExpression != null;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            return Array.Empty<ICompletionSuggestion>();
        }
    }
}
