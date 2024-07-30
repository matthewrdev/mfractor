using System;
using MFractor.Configuration;
using MFractor.Code;
using MFractor.Xml;
using MonoDevelop.Xml.Formatting;

namespace MFractor.VS.Mac
{
	class IdeXmlFormattingPolicy : IXmlFormattingPolicy
	{
        public XmlFormattingPolicy XmlPolicy { get; }

        public IUserOptions UserOptions { get; }

        public IXmlFormattingSettings XmlFormattingSettings { get; }

        public IdeXmlFormattingPolicy(XmlFormattingPolicy xmlPolicy,
                                      IUserOptions userOptions,
                                      IXmlFormattingSettings xmlFormattingSettings)
		{
            XmlPolicy = xmlPolicy;
            UserOptions = userOptions;
            XmlFormattingSettings = xmlFormattingSettings;
        }

        public string[] MimeTypes { get; } = new string[]
        {
            "application/xml",
            "text/xml"
        };

        public string Name => "Visual Studio Mac - Xml Formatting Policy";

		public bool AlignAttributes => XmlPolicy.DefaultFormat.AlignAttributes;

		public bool AlignAttributeValues => XmlPolicy.DefaultFormat.AlignAttributeValues;

        public bool AlignAttributesToFirstAttribute => XmlFormattingSettings.AlignAttributesToFirstAttribute;

        public string AttributesIndentString => XmlFormattingSettings.AttributesIndentString;

        public bool AttributesInNewLine => XmlPolicy.DefaultFormat.AttributesInNewLine;

        public bool FirstAttributeOnNewLine => XmlFormattingSettings.FirstAttributeOnNewLine;

        public string ContentIndentString => XmlFormattingSettings.ContentIndentString;

        public int EmptyLinesAfterEnd => XmlPolicy.DefaultFormat.EmptyLinesAfterEnd;

		public int EmptyLinesAfterStart => XmlPolicy.DefaultFormat.EmptyLinesAfterStart;

		public int EmptyLinesBeforeEnd => XmlPolicy.DefaultFormat.EmptyLinesBeforeEnd;

		public int EmptyLinesBeforeStart => XmlPolicy.DefaultFormat.EmptyLinesBeforeStart;

		public bool IndentContent => XmlPolicy.DefaultFormat.IndentContent;

		public int MaxAttributesPerLine => XmlPolicy.DefaultFormat.MaxAttributesPerLine;

        public string NewLineChars => XmlFormattingSettings.NewLineChars;

		public bool OmitXmlDeclaration => XmlPolicy.DefaultFormat.OmitXmlDeclaration;

		public char QuoteChar => XmlPolicy.DefaultFormat.QuoteChar;

		public int SpacesAfterAssignment => XmlPolicy.DefaultFormat.SpacesAfterAssignment;

		public int SpacesBeforeAssignment => XmlPolicy.DefaultFormat.SpacesBeforeAssignment;

		public bool WrapAttributes => XmlPolicy.DefaultFormat.WrapAttributes;

        public bool AppendSpaceBeforeSlashOnSelfClosingTag => XmlFormattingSettings.AppendSpaceBeforeSlashOnSelfClosingTag;
    }
}