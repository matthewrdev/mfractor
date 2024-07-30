using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class PropertyCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "Property Setter Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var node = context.GetSyntax<XmlNode>();

            if (!CompletionHelper.IsOnOpeningTag(node, textView.TextBuffer, triggerLocation.Position))
            {
                return false;
            }

            var parentNode = node.Parent;

            var type = context.XamlSemanticModel.GetSymbol(parentNode) as INamedTypeSymbol;

            if (SymbolHelper.DerivesFrom(type, context.Platform.OnPlatform.MetaType))
            {
                return false; // Do not suggest properties for on platform, let the shorthand sevice do it.
            }

            var properties = GetCompletionProperties(type, context.Platform);

            if (!properties.Any())
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var parentNode = context.GetSyntax<XmlNode>().Parent;

            var type = context.XamlSemanticModel.GetSymbol(parentNode) as INamedTypeSymbol;

            var properties = GetCompletionProperties(type, context.Platform);

            var items = new List<ICompletionSuggestion>();
            foreach (var property in properties)
            {
                token.ThrowIfCancellationRequested();
                var insertion = GetInsertionText(property, type, context.Project, context.XmlnsDefinitions, context.Namespaces, out var caretOffset);

                var item = new CompletionSuggestion(property.Name, insertion);
                item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, property.Name + " (" + property.Type + ") from " + type.ToString());
                items.Add(item);
            }

            return items;
        }

        IEnumerable<IPropertySymbol> GetCompletionProperties(INamedTypeSymbol type, IXamlPlatform platform)
        {
            var properties = SymbolHelper.GetAllMemberSymbols<IPropertySymbol>(type);
            var fields = SymbolHelper.GetAllMemberSymbols<IFieldSymbol>(type);

            var bindable = fields.Where(p => SymbolHelper.DerivesFrom(p.Type, platform.BindableProperty.MetaType) && p.Name.EndsWith("Property", StringComparison.Ordinal) && p.IsStatic).Select(bp => bp.Name.Substring(0, bp.Name.Length - "Property".Length));

            var bindableProperties = properties.Where(p => bindable.Any(bp => p.Name == bp));

            var result = new List<IPropertySymbol>();

            foreach (var property in properties)
            {
                if (bindable.Any(bp => property.Name == bp))
                {
                    result.Add(property);
                }
                else if (property.SetMethod != null
                         && property.SetMethod.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public
                         && !property.ExplicitInterfaceImplementations.Any())
                {
                    result.Add(property);
                }
                else if (property.Name == "GestureRecognizers" // GestureRecognizers is usable but does not have a public setter. 
                         || property.Name == "Effects") // Effects is usable but does not have a public setter.
                {
                    result.Add(property);
                }
            }

            return result;
        }

        string GetInsertionText(IPropertySymbol property, INamedTypeSymbol parentSymbol, Project project, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlNamespaceCollection xamlNamespaces, out int caretOffset)
        {
            caretOffset = -1;

            var typePrefix = PrefixHelper.GetPrefixForType(parentSymbol, project, xmlnsDefinitions, xamlNamespaces);

            var typeName = $"{typePrefix}{parentSymbol.Name}";

            var text = $"{typeName}.{property.Name}";

            return CompletionHelper.ExtractCaretLocation(text, out caretOffset);
        }
    }
}
