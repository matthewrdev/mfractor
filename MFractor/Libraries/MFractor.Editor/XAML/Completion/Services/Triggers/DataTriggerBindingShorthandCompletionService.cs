using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Attributes;
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
    [TargetXamlPlatform(XamlPlatform.XamarinForms)]
    [TargetXamlPlatform(XamlPlatform.Maui)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class DataTriggerBindingShorthandCompletionService : IXamlCompletionService
    {
        readonly Lazy<IBindingContextResolver> bindingContextResolver;
        public IBindingContextResolver BindingContextResolver => bindingContextResolver.Value;

        public string AnalyticsEvent => "DataTrigger Binding Shorthand Completion";

        [ImportingConstructor]
        public DataTriggerBindingShorthandCompletionService(Lazy<IBindingContextResolver> bindingContextResolver)
        {
            this.bindingContextResolver = bindingContextResolver;
        }

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            if (!context.Platform.SupportsTriggers)
            {
                return false;
            }

            var node = context.GetSyntax<XmlNode>();

            // Is this contained within a <[Type].Triggers> setter?
            if (node == null || !node.HasParent)
            {
                return false;
            }

            // Is the <[Type].Triggers> setter contained by an outer node?
            var triggerSetter = node.Parent;
            if (!triggerSetter.HasParent)
            {
                return false;
            }

            var targetType = context.XamlSemanticModel.GetSymbol(triggerSetter.Parent) as INamedTypeSymbol;
            if (targetType == null)
            {
                return false;
            }

            if (!XamlSyntaxHelper.ExplodePropertySetter(triggerSetter, out _, out var propertyName))
            {
                return false;
            }

            var triggersCollectionType = $"System.Collections.Generic.IList<{context.Platform.TriggerBase.MetaType}>";

            var property = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(targetType, propertyName);
            if (property == null
                || property.Name != "Triggers"
                || !SymbolHelper.DerivesFrom(property.Type, triggersCollectionType))
            {
                return false;
            }

            var bindingContext = BindingContextResolver.ResolveBindingContext(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, node);
            if (bindingContext == null)
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>();

            var node = context.GetSyntax<XmlNode>();
            var triggerSetter = node.Parent.Parent;

            var dataTriggerType = context.Compilation.GetTypeByMetadataName(context.Platform.DataTrigger.MetaType);
            var targetType = context.XamlSemanticModel.GetSymbol(triggerSetter) as INamedTypeSymbol;

            var bindingContext = BindingContextResolver.ResolveBindingContext(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, node);

            var publicMembers = SymbolHelper.GetAllMemberSymbols<IPropertySymbol>(bindingContext).Where(p => p.DeclaredAccessibility.HasFlag(Microsoft.CodeAnalysis.Accessibility.Public)).ToList();

            var namespaces = context.Namespaces;

            foreach (var property in publicMembers)
            {
                token.ThrowIfCancellationRequested();
                var insertion = GetInsertionText(property, dataTriggerType, targetType, context.Project, context.Platform, context.Namespaces, context.XmlnsDefinitions, out var caretOffset);

                var item = new CompletionSuggestion(property.Name + " (Trigger)", insertion);
                item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, $"Creates a new data trigger that targets {property.Name} on {property.ContainingType.ToString()}\n\nInserts:\n{insertion}");
                item.AddProperty(XamlCompletionItemPropertyKeys.CaretOffset, caretOffset);
                items.Add(item);
            }

            return items;
        }

        string GetInsertionText(IPropertySymbol bindingProperty, INamedTypeSymbol dataTriggerType, INamedTypeSymbol targetType, Project project, IXamlPlatform platform, IXamlNamespaceCollection xamlNamespaces, IXmlnsDefinitionCollection xmlnsDefinitions, out int caretOffset)
        {
            caretOffset = -1;

            var prefix = PrefixHelper.GetPrefixForType(targetType, project, xmlnsDefinitions, xamlNamespaces);

            var typePrefix = PrefixHelper.GetPrefixForType(dataTriggerType, project, xmlnsDefinitions, xamlNamespaces);

            var typeName = $"{typePrefix}{dataTriggerType.Name}";

            var text = $"{typeName} TargetType=\"{prefix}{targetType.Name}\" Binding=\"{{{platform.BindingExtension.MarkupExpressionName} {bindingProperty.Name}}}\" Value=\"{CompletionHelper.CaretLocationMarker}\"></{typeName}>";

            return CompletionHelper.ExtractCaretLocation(text, out caretOffset);
        }
    }
}
