using System;

namespace MFractor.Xml
{
    /// <summary>
    /// An implementation of <see cref="IXmlFormattingPolicy"/> that provides sensible default values.
    /// </summary>
    public class DefaultXmlFormattingPolicy : IXmlFormattingPolicy
    {
        public static readonly DefaultXmlFormattingPolicy Instance = new DefaultXmlFormattingPolicy();

        public string[] MimeTypes { get; } = { "application/xml", "text/xml" };

        public string Name => "Default Xml Formatting Policy";

        public bool AlignAttributes => true;

        public bool AlignAttributeValues => true;

        public string AttributesIndentString => "  ";

        public bool AlignAttributesToFirstAttribute => true;

        public bool FirstAttributeOnNewLine => false;

        public bool AttributesInNewLine => false;

        public string ContentIndentString => "    ";

        public int EmptyLinesAfterEnd => 0;

        public int EmptyLinesAfterStart => 0;

        public int EmptyLinesBeforeEnd => 0;

        public int EmptyLinesBeforeStart => 0;

        public bool IndentContent => true;

        public int MaxAttributesPerLine => int.MaxValue;

        public string NewLineChars => "\n";

        public bool OmitXmlDeclaration => false;

        public char QuoteChar => '"';

        public int SpacesAfterAssignment => 0;

        public int SpacesBeforeAssignment => 0;

        public bool WrapAttributes => true;

        public bool AppendSpaceBeforeSlashOnSelfClosingTag => false;
    }
}
