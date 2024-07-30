using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Editor.Tooltips;
using Xwt.Drawing;

namespace MFractor.VS.Mac.Views
{
    public class CodeIssueView : NSStackView
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        NSTextField summaryLabel;

        NSBrandedFooter footer;

        public Color Color { get; }

        public ICodeIssue CodeIssue { get; private set; }
        public IReadOnlyList<ICodeActionSuggestion> Suggestions { get; set; }
        public Action<CodeIssueFixSelectedEventArgs> CodeFixSelected { get; set; }
        public string HelpUrl { get; private set; }

        public CodeIssueView() : base()
        {
            Initialise(CodeIssue, Suggestions, CodeFixSelected, HelpUrl);
        }

        public CodeIssueView(ICodeIssue codeIssue,
                             IReadOnlyList<ICodeActionSuggestion> suggestions,
                             Action<CodeIssueFixSelectedEventArgs> codeFixSelected,
                             string helpUrl)
            : base()
        {

            Initialise(codeIssue, suggestions, codeFixSelected, helpUrl);
        }

        public CodeIssueView(IntPtr handle) : base(handle)
        {
            Initialise(CodeIssue, Suggestions, CodeFixSelected, HelpUrl);
        }

        [Export("initWithFrame:")]
        public CodeIssueView(CGRect frameRect) : base(frameRect)
        {
            Initialise(CodeIssue, Suggestions, CodeFixSelected, HelpUrl);
        }

        public void Initialise(ICodeIssue codeIssue,
                               IReadOnlyList<ICodeActionSuggestion> suggestions,
                               Action<CodeIssueFixSelectedEventArgs> codeFixSelected,
                               string helpUrl)
        {
            try
            {
                CodeIssue = codeIssue;
                Suggestions = suggestions;
                CodeFixSelected = codeFixSelected;
                HelpUrl = helpUrl;

                this.ClearChildren();

                Orientation = NSUserInterfaceLayoutOrientation.Vertical;
                Alignment = NSLayoutAttribute.Left;

                summaryLabel = new NSTextField()
                {
                    Editable = false,
                    Selectable = false,
                    Bezeled = false,
                    DrawsBackground = false,
                    LineBreakMode = NSLineBreakMode.ByWordWrapping,
                    Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
                    StringValue = codeIssue?.Message ?? string.Empty
                };
                InsertView(summaryLabel, (uint)Views.Length, NSStackViewGravity.Top);

                if (suggestions != null && suggestions.Any())
                {
                    InsertView(new NSTextField
                    {
                        Editable = false,
                        Selectable = false,
                        Bezeled = false,
                        Font = NSFont.BoldSystemFontOfSize(NSFont.SystemFontSize),
                        BackgroundColor = NSColor.Clear,
                        StringValue = "Available Fixes:"
                    }, (uint)Views.Length, NSStackViewGravity.Top);

                    foreach (var suggestion in suggestions)
                    {
                        var description = suggestion.Description ?? string.Empty;
                        if (!description.EndsWith("."))
                        {
                            description += ".";
                        }

                        var fixButton = AppKitHelper.CreateLinkButton(description, () => CodeFixSelected?.Invoke(new CodeIssueFixSelectedEventArgs(suggestion)));
                        InsertView(fixButton, (uint)Views.Length, NSStackViewGravity.Top);
                    }
                }

                InsertView(new NSBox()
                {
                    BoxType = NSBoxType.NSBoxSeparator,

                }, (uint)Views.Length, NSStackViewGravity.Top);

                footer = new NSBrandedFooter(HelpUrl);

                InsertView(footer, (uint)Views.Length, NSStackViewGravity.Top);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
