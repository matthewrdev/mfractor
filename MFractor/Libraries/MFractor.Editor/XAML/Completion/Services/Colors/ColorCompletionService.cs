using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Threading;
using MFractor.Editor.Tooltips;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Ide;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using MFractor.Maui.Utilities;

namespace MFractor.Editor.XAML.Completion.Services.Colors
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ColorCompletionService : IXamlCompletionService
    {
        readonly Lazy<IIdeImageManager> ideImageManager;
        public IIdeImageManager IdeImageManager => ideImageManager.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public ColorCompletionService(Lazy<IIdeImageManager> iconService,
                                      Lazy<IWorkEngine> workEngine)
        {
            this.ideImageManager = iconService;
            this.workEngine = workEngine;
        }

        public string AnalyticsEvent => "Color Value Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            if (xamlExpression != null)
            {
                return false;
            }

            if (!CompletionHelper.IsWithinAttributeValue(context, textView, triggerLocation))
            {
                return false;
            }

            var isColorContext = IsColorContext(context);

            return isColorContext;
        }

        bool IsColorContext(IXamlFeatureContext context)
        {
            var returnType = GetAttributeMemberReturnType(context);

            return FormsSymbolHelper.IsColor(returnType, context.Platform);
        }

        ITypeSymbol GetAttributeMemberReturnType(IXamlFeatureContext context)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null)
            {
                return null;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(attribute);

            return SymbolHelper.ResolveMemberReturnType(symbol);
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>();
            var type = GetAttributeMemberReturnType(context);

            var action = CreateColorPickerCompletion(context.Document.FilePath);
            items.Add(action);

            var fields = SymbolHelper.GetAllMemberSymbols<IFieldSymbol>(type)
                            .Where(f => f.IsReadOnly && f.IsStatic && f.DeclaredAccessibility.HasFlag(Microsoft.CodeAnalysis.Accessibility.Public));

            foreach (var f in fields)
            {
                token.ThrowIfCancellationRequested();
                ImageElement icon = default;

                var colorName = "color-" + f.Name.ToLower();
                if (IdeImageManager.HasImage(colorName))
                {
                    var guid = IdeImageManager.GetGuid(colorName);
                    var id = guid.GetHashCode();

                    icon = new ImageElement(new ImageId(guid, id), f.Name.ToLower());
                }

                var item = new CompletionSuggestion(f.Name, f.Name, icon);
                try
                {
                    var color = ColorTranslator.FromHtml(f.Name);

                    item.AddProperty(XamlCompletionItemPropertyKeys.TooltipModel, new ColorTooltipModel(color));
                }
                catch { }
                items.Add(item);
            }

            return items;
        }

        ICompletionSuggestion CreateColorPickerCompletion(string filePath)
        {
            var actionName = "Choose a color";
            var action = new CompletionSuggestion(actionName, actionName, new ImageElement(IconIds.ColorPicker));
            action.AddProperty(XamlCompletionItemPropertyKeys.CompletionAction, new CompletionAction((tv, t, ci) =>
            {
                void onColorSelected(Color color)
                {
                    var span = TextEditorHelper.GetAttributeSpanAtOffset(t, tv.GetCaretOffset());

                    var textWorkUnit = new ReplaceTextWorkUnit()
                    {
                        Span = span,
                        Text = HexConverter(color),
                        FilePath = filePath,
                    };

                    WorkEngine.ApplyAsync(textWorkUnit);
                }

                return new ColorEditorWorkUnit(Color.Black, onColorSelected).AsList();
            }));

            action.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, $"Open the color picker to visually choose a color.");
            action.AddProperty(XamlCompletionItemPropertyKeys.AnalyticsEvent, "Color Picker Completion");
            return action;
        }

        static string HexConverter(System.Drawing.Color color)
        {
            if (color.IsNamedColor)
            {
                return color.Name;
            }

            return ColorHelper.GetHexString(color, true);
        }
    }
}
