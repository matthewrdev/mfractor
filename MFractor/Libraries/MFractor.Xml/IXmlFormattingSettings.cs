namespace MFractor.Xml
{
    public interface IXmlFormattingSettings
    {
        string AttributesIndentString { get; set; }

        string ContentIndentString { get; set; }

        string NewLineChars { get; set; }

        bool AlignAttributesToFirstAttribute { get; set; }

        bool FirstAttributeOnNewLine { get; set; }

        bool AppendSpaceBeforeSlashOnSelfClosingTag { get; set; }
    }
}