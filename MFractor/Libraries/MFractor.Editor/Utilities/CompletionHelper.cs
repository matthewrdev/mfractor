using System;
using MFractor.Maui;
using MFractor.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class CompletionHelper
    {
        public const string CaretLocationMarker = "<|>";

        public static string ExtractCaretLocation(string input, out int caretOffset)
        {
            caretOffset = -1;

            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (!input.Contains(CaretLocationMarker))
            {
                return input;
            }

            caretOffset = input.IndexOf(CaretLocationMarker, StringComparison.Ordinal);

            return input.Replace(CaretLocationMarker, string.Empty);
        }

        public static bool IsWithinAttributeValue(IXamlFeatureContext context, ITextBuffer textBuffer, int position)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null)
            {
                return false;
            }

            return IsWithinAttributeValue(attribute, textBuffer, position);
        }

        public static bool IsWithinAttributeValue(IXamlFeatureContext context, ITextView textView, SnapshotPoint triggerLocation)
        {
            return IsWithinAttributeValue(context, textView.TextBuffer, triggerLocation.Position);
        }

        public static bool IsWithinAttributeValue(XmlAttribute attribute, ITextBuffer textBuffer, int position)
        {
            if (attribute == null)
            {
                return false;
            }

            if (attribute.Value == null)
            {
                return false;
            }

            if (attribute.Value.IsClosed)
            {
                return attribute.Value.Span.IntersectsWith(position);
            }
            else
            {
                return attribute.Value.Span.Start <= position;
            }
        }

        public static bool IsOnOpeningTag(XmlNode node, ITextBuffer textBuffer, int position)
        {
            if (node == null || textBuffer == null)
            {
                return false;
            }

            if (!node.OpeningTagSpan.IntersectsWith(position))
            {
                return false;
            }

            if (node.HasClosingTag && node.ClosingTagNameSpanValid)
            {
                return false;
            }

            var lastCharacter = textBuffer.CurrentSnapshot.GetText(position - 1, 1);

            // Must be on the tag exactly.
            if (string.IsNullOrWhiteSpace(lastCharacter))
            {
                return false;
            }

            var currentCharacter = textBuffer.CurrentSnapshot.GetText(position, 1);

            return true;
        }

        /// <summary>
        /// For the given <paramref name="featureContext"/> and <paramref name="triggerLocation"/>, deduces the valid <see cref="XmlNode"/> for a shorthand completion.
        /// </summary>
        public static XmlNode ResolveShorthandCompletionTarget(IXamlFeatureContext featureContext, ITextBuffer textBuffer, SnapshotPoint triggerLocation)
        {
            var node = featureContext.GetSyntax<XmlNode>();

            // If the current node is null, then we are inside an attribute completion (promising!).
            // We need to check if we are in the value span
            if (node == null)
            {
                var attribute = featureContext.GetSyntax<XmlAttribute>();

                if (attribute == null || attribute.Name == null)
                {
                    return null;
                }

                // Are we in the 
                if (attribute.Value != null)
                {
                    // Are we within the bounds of the value?
                    if (attribute.HasValue
                        && attribute.Value.Span.IntersectsWith(triggerLocation.Position))
                    {
                        return null;
                    }

                    // Is the past the end of the value
                    if (triggerLocation.Position >= attribute.Value.Span.End)
                    {
                        return null;
                    }
                }

                node = attribute.Parent;
            }
            // Check if we are in the name span of the node, if we are then we on an XML named element completion and not an attribute shorthand.
            else if (node.NameSpan.IntersectsWith(triggerLocation.Position))
            {
                return null;
            }
            // Check if we are outside the bounds of the opening tag span. If we are, then we are in the XML nodes value or closing tag and not an attribute short hand completion.
            else if (!node.OpeningTagSpan.IntersectsWith(triggerLocation.Position))
            {
                return null;
            }
            else if (node.OpeningTagSpan.End == triggerLocation.Position
                     && node.HasClosingTag)
            {
                if (node.ClosingTagNameSpanValid)
                {
                    return null;
                }
            }
            else if (triggerLocation.Position <= node.NameSpan.Start) // We are somehow before the nodes name. This is invalid and we do not want to suggest attribute completions.
            {
                return null;
            }

            var lastCharacter = textBuffer.CurrentSnapshot.GetText(triggerLocation.Position - 1, 1);

            // If the last character is whitespace, we are definitely in a shorthand completion
            if (string.IsNullOrWhiteSpace(lastCharacter))
            {
                return node;
            }

            // If the last character is an equals, then the user is beginning a value assignment and the shorthand completion is not relevant.
            // If the last character is a '"', then the user has closed an attribute.
            if (lastCharacter == "=" || lastCharacter == "\"")
            {
                return null;
            }

            // All other characters are relevant.
            return node;
        }
    }
}
