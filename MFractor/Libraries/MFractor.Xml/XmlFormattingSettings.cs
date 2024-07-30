using System;
using System.ComponentModel.Composition;
using MFractor.Configuration;

namespace MFractor.Xml
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXmlFormattingSettings))]
    class XmlFormattingSettings : IXmlFormattingSettings
    {
        readonly Lazy<IUserOptions> userOptions;
        IUserOptions UserOptions => userOptions.Value;

        [ImportingConstructor]
        public XmlFormattingSettings(Lazy<IUserOptions> userOptions)
        {
            this.userOptions = userOptions;
        }

        /// <summary>
        /// The key for <see cref="AttributesIndentString"/>.
        /// </summary>
        public const string AttributesIndentStringKey = "com.mfractor.formatting.xml.attributes_indent_string";

        public string AttributesIndentString
        {
            get => UserOptions.Get(AttributesIndentStringKey, "  ");
            set => UserOptions.Set(AttributesIndentStringKey, value);
        }

        public const string ContentIndentStringKey = "com.mfractor.formatting.xml.content_indent_string";

        public string ContentIndentString
        {
            get => UserOptions.Get(ContentIndentStringKey, "    ");
            set => UserOptions.Set(ContentIndentStringKey, value);
        }

        public const string NewLineCharsKey = "com.mfractor.formatting.xml.new_line_chars";

        public string NewLineChars
        {
            get => UserOptions.Get(NewLineCharsKey, Environment.NewLine);
            set => UserOptions.Set(NewLineCharsKey, value);
        }

        public const string AlignAttributesToFirstAttributeKey = "com.mfractor.formatting.xml.align_attributes_to_first_attribute";

        public bool AlignAttributesToFirstAttribute
        {
            get => UserOptions.Get(AlignAttributesToFirstAttributeKey, true);
            set => UserOptions.Set(AlignAttributesToFirstAttributeKey, value);
        }

        public const string FirstAttributeOnNewLineKey = "com.mfractor.formatting.xml.first_attribute_on_newline";

        public bool FirstAttributeOnNewLine
        {
            get => UserOptions.Get(FirstAttributeOnNewLineKey, false);
            set => UserOptions.Set(FirstAttributeOnNewLineKey, value);
        }

        public const string AppendSpaceBeforeSlashOnSelfClosingTagKey = "com.mfractor.formatting.xml.append_space_before_slash_on_self_closing_tag";

        public bool AppendSpaceBeforeSlashOnSelfClosingTag
        {
            get => UserOptions.Get(AppendSpaceBeforeSlashOnSelfClosingTagKey, false);
            set => UserOptions.Set(AppendSpaceBeforeSlashOnSelfClosingTagKey, value);
        }
    }
}
