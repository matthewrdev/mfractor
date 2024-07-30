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

namespace MFractor.Editor.XAML.Completion.Services.OnPlatform
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class PlatformShorthandCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "OnPlatform Platform Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var node = context.GetSyntax<XmlNode>();

            if (!CompletionHelper.IsOnOpeningTag(node, textView.TextBuffer, triggerLocation.Position))
            {
                return default;
            }

            var setterParent = node.Parent;
            var type = context.XamlSemanticModel.GetSymbol(setterParent) as INamedTypeSymbol;

            return SymbolHelper.DerivesFrom(type, context.Platform.OnPlatform.MetaType);
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var completions = GetPlatformCompletions(context.Compilation, context.Platform);

            return completions;
        }

        public IReadOnlyList<ICompletionSuggestion> GetPlatformCompletions(Compilation compilation, IXamlPlatform platform)
        {
            var items = new List<ICompletionSuggestion>();

            var device = compilation.GetTypeByMetadataName(platform.Device.MetaType);

            var fields = SymbolHelper.GetAllMemberSymbols<IFieldSymbol>(device)
                             .Where(f => f.Type.SpecialType == SpecialType.System_String && f.IsConst);

            if (!fields.Any())
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            foreach (var field in fields)
            {
                var value = field.ConstantValue.ToString();
                var item = new CompletionSuggestion(value, value);

                var rawInsertion = $"On Platform=\"{value}\" Value=\"{CompletionHelper.CaretLocationMarker}\"/>";

                var insertion = CompletionHelper.ExtractCaretLocation(rawInsertion, out var caretOffset);

                var tooltip = $"Target the {value} platform.\n\nInserts:\n{insertion}";

                var completion = new CompletionSuggestion(field.Name, insertion);

                completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, tooltip)
                          .AddProperty(XamlCompletionItemPropertyKeys.CaretOffset, caretOffset);

                items.Add(completion);
            }

            return items;
        }
    }
}
