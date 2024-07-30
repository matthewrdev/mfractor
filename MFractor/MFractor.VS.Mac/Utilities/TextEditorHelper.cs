using MFractor.Editor.Utilities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.VS.Mac.Utilities
{
    public static class TextEditorHelper
    {
        public static void SetSelection(this MonoDevelop.Ide.Gui.Document document, int start, int end)
        {
            var textView = document.GetContent<ITextView>();
            if (textView != null)
            {
                textView.SetSelection(new SnapshotSpan(textView.TextSnapshot, Span.FromBounds(start, end)));
            }
            //else if (document.Editor != null)
            //{
            //    document.Editor.SetSelection(start, end);
            //}
        }

        public static void SetCaretLocation(this MonoDevelop.Ide.Gui.Document document, int offset)
        {
            var textView = document.GetContent<ITextView>();
            if (textView != null)
            {
                textView.TryMoveCaretToAndEnsureVisible(new SnapshotPoint(textView.TextSnapshot, offset));
            }
            //else if (document.Editor != null)
            //{
            //    var location = document.Editor.OffsetToLocation(offset);
            //    document.Editor.SetCaretLocation(location);
            //}
        }

        public static int GetCaretOffset(this MonoDevelop.Ide.Gui.Document document)
        {
            var textView = document.GetContent<ITextView>();

            if (textView != null)
            {
                return textView.Caret.Position.BufferPosition.Position;
            }

            //if (document.Editor != null)
            //{
            //    return document.Editor.CaretOffset;
            //}

            return 0;
        }

        public static InteractionLocation GetInteractionLocation(this MonoDevelop.Ide.Gui.Document document, int? offset = null)
        {
            var textView = document.GetContent<ITextView>();

            TextSpan? selection = null;
            if (textView != null)
            {
                if (textView.Selection != null)
                {
                    selection = TextSpan.FromBounds(textView.Selection.Start.Position.Position, textView.Selection.End.Position.Position);
                }

                return new InteractionLocation(offset != null ? offset.Value : document.GetCaretOffset(), selection);
            }

            //if (document.Editor != null)
            //{
            //    var editor = document.Editor;

            //    var position = offset != null ? offset.Value : editor.CaretOffset;

            //    var editorSelection = editor.SelectionRegion;

            //    var start = editor.LocationToOffset(editorSelection.Begin);
            //    var end = editor.LocationToOffset(editorSelection.End);

            //    if (end > start)
            //    {
            //        selection = TextSpan.FromBounds(start, end);
            //    }

            //    return new InteractionLocation(position, selection);
            //}

            return new InteractionLocation(0);
        }
    }
}
