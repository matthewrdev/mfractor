using System;

namespace MFractor.Xml
{
    /// <summary>
    /// An Xml formatting policy.
    /// <para/>
    /// To retrieve an XML formatting policy, please see <see cref="ICodeFormattingPolicyService"/>.
    /// </summary>
	public interface IXmlFormattingPolicy
	{
        bool AlignAttributes { get; }

		bool AlignAttributeValues { get; }

        string AttributesIndentString { get; }

        bool AlignAttributesToFirstAttribute { get; }

        bool AttributesInNewLine { get; }

        string ContentIndentString { get; }

        int EmptyLinesAfterEnd { get; }

        int EmptyLinesAfterStart { get; }

        int EmptyLinesBeforeEnd { get; }

		int EmptyLinesBeforeStart { get; }

        bool IndentContent { get; }

        int MaxAttributesPerLine { get; }

        string NewLineChars { get; }

        bool OmitXmlDeclaration { get; }

        char QuoteChar { get; }

        int SpacesAfterAssignment { get; }

        int SpacesBeforeAssignment { get; }

        bool WrapAttributes { get; }

        bool FirstAttributeOnNewLine { get; }

        bool AppendSpaceBeforeSlashOnSelfClosingTag { get; }
        string[] MimeTypes { get; }
        string Name { get; }
    }
}
