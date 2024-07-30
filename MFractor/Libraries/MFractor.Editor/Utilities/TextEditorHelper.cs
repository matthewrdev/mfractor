using System;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Outlining;

namespace MFractor.Editor.Utilities
{
    public static class TextEditorHelper // Based on: https://github.com/dotnet/roslyn/blob/master/src/EditorFeatures/Core/Shared/Extensions/ITextViewExtensions.cs
    {
        readonly static Logging.ILogger log = Logging.Logger.Create();

        public static SnapshotPoint? GetPositionInView(this ITextView textView, SnapshotPoint point)
              => textView.BufferGraph.MapUpToSnapshot(point, PointTrackingMode.Positive, PositionAffinity.Successor, textView.TextSnapshot);


        public static bool TryMoveCaretToAndEnsureVisible(this ITextView textView, SnapshotPoint point, IOutliningManagerService outliningManagerService = null, EnsureSpanVisibleOptions ensureSpanVisibleOptions = EnsureSpanVisibleOptions.None)
            => textView.TryMoveCaretToAndEnsureVisible(new VirtualSnapshotPoint(point), outliningManagerService, ensureSpanVisibleOptions);

        public static bool TryMoveCaretToAndEnsureVisible(this ITextView textView, VirtualSnapshotPoint point, IOutliningManagerService outliningManagerService = null, EnsureSpanVisibleOptions ensureSpanVisibleOptions = EnsureSpanVisibleOptions.None)
        {
            if (textView.IsClosed)
            {
                return false;
            }

            var pointInView = textView.GetPositionInView(point.Position);

            if (!pointInView.HasValue)
            {
                return false;
            }

            // If we were given an outlining service, we need to expand any outlines first, or else
            // the Caret.MoveTo won't land in the correct location if our target is inside a
            // collapsed outline.
            if (outliningManagerService != null)
            {
                var outliningManager = outliningManagerService.GetOutliningManager(textView);

                if (outliningManager != null)
                {
                    outliningManager.ExpandAll(new SnapshotSpan(pointInView.Value, length: 0), match: _ => true);
                }
            }

            var newPosition = textView.Caret.MoveTo(new VirtualSnapshotPoint(pointInView.Value, point.VirtualSpaces));

            // We use the caret's position in the view's current snapshot here in case something 
            // changed text in response to a caret move (e.g. line commit)
            var spanInView = new SnapshotSpan(newPosition.BufferPosition, 0);
            textView.ViewScroller.EnsureSpanVisible(spanInView, ensureSpanVisibleOptions);

            return true;
        }

        public static NormalizedSnapshotSpanCollection GetSpanInView(this ITextView textView, SnapshotSpan span)
            => textView.BufferGraph.MapUpToSnapshot(span, SpanTrackingMode.EdgeInclusive, textView.TextSnapshot);

        public static void SetSelection(
            this ITextView textView, VirtualSnapshotPoint anchorPoint, VirtualSnapshotPoint activePoint)
        {
            var isReversed = activePoint < anchorPoint;
            var start = isReversed ? activePoint : anchorPoint;
            var end = isReversed ? anchorPoint : activePoint;
            SetSelection(textView, new SnapshotSpan(start.Position, end.Position), isReversed);
        }

        public static void SetSelection(
            this ITextView textView, SnapshotSpan span, bool isReversed = false)
        {
            var spanInView = textView.GetSpanInView(span).Single();
            textView.Selection.Select(spanInView, isReversed);
            textView.Caret.MoveTo(isReversed ? spanInView.Start : spanInView.End);
        }

        public static void SetSelection(
            this ITextView textView, TextSpan span, bool isReversed = false)
        {
            var snapshotSpan = new SnapshotSpan(textView.TextSnapshot, Span.FromBounds(span.Start, span.End));

            textView.SetSelection(snapshotSpan, isReversed);
        }

        public static FileLocation GetLocation(this ITextBuffer textBuffer, int position)
        {
            if (TryGetLineAndColumn(textBuffer.CurrentSnapshot, position, out var line, out var column))
            {
                return new FileLocation(line, column);
            }

            return default;
        }

        public static SnapshotSpan FindTokenSpanAtPosition(ITextBuffer textBuffer, int position, ITextStructureNavigatorSelectorService structureNavigatorSelector)
        {
            if (!TryGetLineAndColumn(textBuffer.CurrentSnapshot, position, out var line, out var column))
            {
                return default;
            }

            var location = GetSnapshotPoint(textBuffer.CurrentSnapshot, line, column);

            if (location == null)
            {
                return default;
            }

            return FindTokenSpanAtPosition(location.Value, structureNavigatorSelector);
        }

        public static bool TryGetLineAndColumn(ITextSnapshot snapshot, int position, out int line, out int column)
        {
            line = 0;
            column = 0;

            if (position < 0 || position >= snapshot.Length)
            {
                return false;
            }

            var snapshotLine = snapshot.GetLineFromPosition(position);

            column = position - snapshotLine.Start + 1;
            line = snapshotLine.LineNumber + 1;

            return true;
        }

        /// <summary>
        /// Gets a snapshot point out of a line/column pair or null.
        /// </summary>
        /// <returns>The snapshot point or null if the coordinate is invalid.</returns>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="line">Line number (1 based).</param>
        /// <param name="column">Column number (1 based).</param>
        public static SnapshotPoint? GetSnapshotPoint(ITextSnapshot snapshot, int line, int column)
        {
            if (TryGetSnapshotPoint(snapshot, line, column, out var snapshotPoint))
            {
                return snapshotPoint;
            }
            return null;
        }

        /// <summary>
        /// Tries to get a snapshot point out of a monodevelop line/column pair.
        /// </summary>
        /// <returns><c>true</c>, if get snapshot point was set, <c>false</c> otherwise.</returns>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="line">Line number (1 based).</param>
        /// <param name="column">Column number (1 based).</param>
        /// <param name="snapshotPoint">The snapshot point if return == true.</param>
        public static bool TryGetSnapshotPoint(ITextSnapshot snapshot, int line, int column, out SnapshotPoint snapshotPoint)
        {
            if (line < 1 || line > snapshot.LineCount)
            {
                snapshotPoint = default;
                return false;
            }

            var lineSegment = snapshot.GetLineFromLineNumber(line - 1);
            if (column < 1 || column > lineSegment.LengthIncludingLineBreak)
            {
                snapshotPoint = default;
                return false;
            }

            snapshotPoint = new SnapshotPoint(snapshot, lineSegment.Start.Position + column - 1);
            return true;
        }

        public static SnapshotSpan FindTokenSpanAtPosition(SnapshotPoint location, ITextStructureNavigatorSelectorService structureNavigatorSelector)
        {
            // This method is not really related to completion,
            // we mostly work with the default implementation of ITextStructureNavigator 
            // You will likely use the parser of your language
            var navigator = structureNavigatorSelector.GetTextStructureNavigator(location.Snapshot.TextBuffer);
            var extent = navigator.GetExtentOfWord(location);
            if (location.Position > 0 && (!extent.IsSignificant || !extent.Span.GetText().Any(c => char.IsLetterOrDigit(c))))
            {
                // Improves span detection over the default ITextStructureNavigation result
                extent = navigator.GetExtentOfWord(location - 1);
            }

            var tokenSpan = location.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);

            var snapshot = location.Snapshot;
            var tokenText = tokenSpan.GetText(snapshot);
            if (string.IsNullOrWhiteSpace(tokenText))
            {
                // The token at this location is empty. Return an empty span, which will grow as user types.
                return new SnapshotSpan(location, 0);
            }

            // Trim quotes and new line characters.
            var startOffset = 0;
            var endOffset = 0;

            if (tokenText.Length > 0)
            {
                if (tokenText.StartsWith("\"", StringComparison.Ordinal)
                    || tokenText.StartsWith("<", StringComparison.Ordinal))
                {
                    startOffset = 1;
                }
            }
            if (tokenText.Length - startOffset > 0)
            {
                if (tokenText.EndsWith("\"\r\n", StringComparison.Ordinal))
                {
                    endOffset = 3;
                }
                else if (tokenText.EndsWith("\r\n", StringComparison.Ordinal))
                {
                    endOffset = 2;
                }
                else if (tokenText.EndsWith("\"\n", StringComparison.Ordinal))
                {
                    endOffset = 2;
                }
                else if (tokenText.EndsWith("\n", StringComparison.Ordinal))
                {
                    endOffset = 1;
                }
                else if (tokenText.EndsWith("\"", StringComparison.Ordinal))
                {
                    endOffset = 1;
                }
            }

            // Are we in an empty attribute value?
            if (location.Position - 1 >= 0)
            {
                var previousCharacter = location.Snapshot.GetText(location.Position - 1, 1);
                var currentCharacter = location.Snapshot.GetText(location.Position, 1);

                if (currentCharacter == "\""
                    && previousCharacter == "\"")
                {
                    startOffset = 0;
                    endOffset = 0;
                    tokenSpan = location.Snapshot.CreateTrackingSpan(Span.FromBounds(location.Position, location.Position), SpanTrackingMode.EdgeInclusive);
                }
            }

            return new SnapshotSpan(tokenSpan.GetStartPoint(snapshot) + startOffset, tokenSpan.GetEndPoint(snapshot) - endOffset);
        }

        public static TextSpan GetAttributeSpanAtOffset(ITextBuffer textBuffer, int offset)
        {
            var text = "";
            var start = offset - 1;
            while (!text.StartsWith("\"", StringComparison.OrdinalIgnoreCase))
            {
                text = textBuffer.CurrentSnapshot.GetText(Span.FromBounds(start, offset));
                if (!text.StartsWith("\"", StringComparison.OrdinalIgnoreCase))
                {
                    start--;
                }
            }

            start++;

            text = "";
            var end = offset;
            while (!text.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
            {
                text = textBuffer.CurrentSnapshot.GetText(Span.FromBounds(start, end));
                if (!text.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
                {
                    end++;
                }
            }

            end--;

            return TextSpan.FromBounds(start, end);
        }

        public static void SetCaretLocation(this ITextView textView, int offset)
        {
            textView.TryMoveCaretToAndEnsureVisible(new SnapshotPoint(textView.TextSnapshot, offset));
        }

        public static int GetCaretOffset(this ITextView textView)
        {
            return textView.Caret.Position.BufferPosition.Position;
        }

        public static InteractionLocation GetInteractionLocation(this ITextView textView, int? offset = null)
        {
            TextSpan? selection = null;

            if (textView.Selection != null)
            {
                selection = TextSpan.FromBounds(textView.Selection.Start.Position.Position, textView.Selection.End.Position.Position);
            }

            return new InteractionLocation(offset != null ? offset.Value : textView.GetCaretOffset(), selection);
        }

        public static void ReplaceText(this ITextBuffer textBuffer, int offset, int length, string text)
        {
            textBuffer.Replace(Span.FromBounds(offset, offset + length), text);
        }

        public static TextSpan GetCurrentNodeNameSpanAtOffset(this ITextBuffer textBuffer, int offset)
        {
            try
            {
                var text = "";
                var start = offset - 1;
                while (!text.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                {
                    var length = offset - start;
                    text = textBuffer.CurrentSnapshot.GetText(start, length);
                    if (!text.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                    {
                        start--;
                    }
                }
                start++;

                text = "";
                var end = offset;
                while (!(text.EndsWith(" ", StringComparison.OrdinalIgnoreCase) || text.EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase)))
                {
                    text = textBuffer.CurrentSnapshot.GetText(start, end - start);
                    if (!(text.EndsWith(" ", StringComparison.OrdinalIgnoreCase) || text.EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase)))
                    {
                        end++;
                    }
                }

                end--;

                return TextSpan.FromBounds(start, end);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return TextSpan.FromBounds(0, 0);
        }

        public static TextSpan GetCurrentXmlValueSpan(this ITextBuffer textBuffer, int offset)
        {
            var text = "";
            var start = offset - 1;
            while (!text.StartsWith(">", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("\n", StringComparison.OrdinalIgnoreCase))
            {
                text = textBuffer.CurrentSnapshot.GetText(start, start + offset);

                if (text.StartsWith(">", StringComparison.OrdinalIgnoreCase) || text.StartsWith("\n", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                start--;
            }

            start++;

            text = "";
            var end = offset;
            while (!text.EndsWith("<", StringComparison.OrdinalIgnoreCase) && !text.EndsWith("\n", StringComparison.OrdinalIgnoreCase))
            {
                text = textBuffer.CurrentSnapshot.GetText(start, end - start);

                if (text.EndsWith("<", StringComparison.OrdinalIgnoreCase) || text.EndsWith("\n", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                end++;
            }

            end--;

            return TextSpan.FromBounds(start, end);
        }

        /// <summary>
        /// Get's the first non-whitespace character in the <paramref name="textBuffer"/> that comes before the <paramref name="offset"/>.
        /// </summary>
        public static string GetFirstNonWhitespacePreceedingCharacter(this ITextBuffer textBuffer, int offset)
        {
            for (; offset > 0; --offset)
            {
                var character = textBuffer.CurrentSnapshot.GetText(offset, 1);

                if (!string.IsNullOrWhiteSpace(character))
                {
                    return character;
                }
            }

            return string.Empty;
        }
    }
}
