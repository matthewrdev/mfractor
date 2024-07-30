using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SetterShorthandCompletionService : IXamlCompletionService
    {
        readonly Lazy<ITargetTypeSymbolResolver> targetTypeSymbolResolver;
        public ITargetTypeSymbolResolver TargetTypeSymbolResolver => targetTypeSymbolResolver.Value;

        public string AnalyticsEvent => "Setter Shorthand Completion";

        [ImportingConstructor]
        public SetterShorthandCompletionService(Lazy<ITargetTypeSymbolResolver> targetTypeSymbolResolver)
        {
            this.targetTypeSymbolResolver = targetTypeSymbolResolver;
        }

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var targetType = GetSetterTargetType(context, textView, triggerLocation);

            var isAvailable = targetType != null && targetType.Success;

            return isAvailable;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var targetType = GetSetterTargetType(context, textView, triggerLocation);

            var completions = GetSetterPropertyShortHandCompletions(targetType.TargetType, context.Platform, context.Compilation);

            return completions;
        }

        public TargetTypeSymbolResult GetSetterTargetType(IXamlFeatureContext context, ITextView textView, SnapshotPoint triggerLocation)
        {
            var node = context.GetSyntax<XmlNode>();

            if (!CompletionHelper.IsOnOpeningTag(node, textView.TextBuffer, triggerLocation.Position))
            {
                return default;
            }

            // Do not provide completions for xmlns usages/
            if (node.Name.FullName.Contains(":"))
            {
                return default;
            }

            var setterParent = node.Parent;
            var type = context.XamlSemanticModel.GetSymbol(setterParent) as INamedTypeSymbol;

            var isCandidate = SymbolHelper.DerivesFrom(type, context.Platform.Style.MetaType) || SymbolHelper.DerivesFrom(type, context.Platform.TriggerBase.MetaType);
            if (!isCandidate)
            {
                return default;
            }

            return TargetTypeSymbolResolver.GetTargetTypeSymbolForNode(setterParent, context.Project, context.Namespaces, context.XmlnsDefinitions, "TargetType", false);
        }

        public IReadOnlyList<ICompletionSuggestion> GetSetterPropertyShortHandCompletions(INamedTypeSymbol type, IXamlPlatform platform, Compilation compilation)
        {
            var items = new List<ICompletionSuggestion>();

            var props = SymbolHelper.GetAllMemberSymbols<IPropertySymbol>(type)
                             .Where(p => p.SetMethod != null)
                             .Where(p => p.SetMethod.DeclaredAccessibility.HasFlag(Microsoft.CodeAnalysis.Accessibility.Public));

            if (!props.Any())
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var setterSymbol = compilation.GetTypeByMetadataName(platform.Setter.MetaType);

            if (setterSymbol == null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            foreach (var property in props)
            {
                var item = new CompletionSuggestion(property.Name + " (Setter)", property.Name);

                var rawInsertion = $"Setter Property=\"{property.Name}\" Value=\"{CompletionHelper.CaretLocationMarker}\"/>";

                var insertion = CompletionHelper.ExtractCaretLocation(rawInsertion, out var caretOffset);

                var tooltip = $"Create a Setter that accesses {property.Name} from {type}\n\nInserts:\n{insertion}";

                var completion = new CompletionSuggestion(property.Name, insertion);

                completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, tooltip)
                          .AddProperty(XamlCompletionItemPropertyKeys.CaretOffset, caretOffset);

                items.Add(completion);
            }

            return items;
        }
    }
}
