using System;
using System.ComponentModel.Composition;
using MFractor.Attributes;
using MFractor.Configuration;
using MFractor.IOC;
using MFractor.Utilities;
using MFractor.Xml;
using Xwt;

namespace MFractor.Views.Settings
{
    public class FormattingOptionsWidget : VBox, IOptionsWidget
    {
        [Import]
        IUserOptions UserOptions { get; set; }

        [Import]
        IXmlFormattingSettings XmlFormattingSettings { get; set; }

        enum WhitespaceMode
        {
            [Description("Tabs")]
            Tabs,

            [Description("Spaces")]
            Spaces,
        }

        const string whitespaceModeKey = "com.mfractor.settings.whitespace_mode";

        const string whitespaceCharacterCountKey = "com.mfractor.settings.whitespace_character_count";

        ComboBox whitespaceModeComboBox;

        TextEntry whitespaceCharacterCountEntry;

        CheckBox alignAttributesToFirstAttribute;

        CheckBox placeFirstAttributeOnNewLine;

        CheckBox appendSpaceBeforeSlashOnSelfClosingTag;

        public Widget Widget => this;

        public string Title => "Formatting";

        public FormattingOptionsWidget()
        {
            Resolver.ComposeParts(this);

            Build();
        }

        const string whitespaceCharacterModeTooltipText = "What whitespace character should MFractor use when applying formatting?";
        const string whitespaceCharacterCountTooltipText = "How many whitespace characters should MFractor use when applying formatting?";

        void Build()
        {
            alignAttributesToFirstAttribute = new CheckBox
            {
                Label = "Align Attributes To First Attribute",
                Active = XmlFormattingSettings.AlignAttributesToFirstAttribute,
                TooltipText = "When attributes are split onto new lines in XML, should these attributes be aligned with the first attribute that appears after the XML element declaration?"
            };

            PackStart(alignAttributesToFirstAttribute);

            placeFirstAttributeOnNewLine = new CheckBox
            {
                Label = "Place First Attribute On New Line",
                Active = XmlFormattingSettings.FirstAttributeOnNewLine,
                TooltipText = "Should MFractor place the first XML attribute onto the same line as the XML element or on a new line?"
            };

            PackStart(placeFirstAttributeOnNewLine);

            appendSpaceBeforeSlashOnSelfClosingTag = new CheckBox
            {
                Label = "Append space before slash on self closing tags",
                Active = XmlFormattingSettings.AppendSpaceBeforeSlashOnSelfClosingTag,
                TooltipText = "Should MFractor place a space between the last attribute and the closing slash (/>) when formatting a self closing tag?",
            };

            PackStart(appendSpaceBeforeSlashOnSelfClosingTag);

            PackStart(new Label("Whitespace Character")
            {
                TooltipText = whitespaceCharacterModeTooltipText,
            });

            whitespaceModeComboBox = new ComboBox()
            {
                TooltipText = whitespaceCharacterModeTooltipText
            };

            var values = EnumHelper.GetDisplayValues<WhitespaceMode>();

            foreach (var v in values)
            {
                whitespaceModeComboBox.Items.Add(v.Item2, v.Item1);
            }

            whitespaceModeComboBox.SelectedItem = (int)Mode;

            PackStart(whitespaceModeComboBox);

            PackStart(new Label("Whitespace Character Amount")
            {
                TooltipText = whitespaceCharacterCountTooltipText,
            });

            whitespaceCharacterCountEntry = new TextEntry();
            whitespaceCharacterCountEntry.Text = WhitespaceCharacterCount.ToString();
            whitespaceCharacterCountEntry.TooltipText = whitespaceCharacterCountTooltipText;
            whitespaceCharacterCountEntry.TextInput += (sender, e) =>
            {
                if (!int.TryParse(e.Text, out var number) || number < 0 || number > 1000)
                {
                    e.Handled = true;
                }
            };

            PackStart(whitespaceCharacterCountEntry);
        }

        public void ApplyChanges()
        {
            using (var transaction = UserOptions.StartTransaction())
            {
                UserOptions.Set(whitespaceModeKey, (int)whitespaceModeComboBox.SelectedItem);
                UserOptions.Set(whitespaceCharacterCountKey, int.Parse(whitespaceCharacterCountEntry.Text));

                var whitespace = GetWhitespace();

                XmlFormattingSettings.AttributesIndentString = whitespace;
                XmlFormattingSettings.ContentIndentString = whitespace;
                XmlFormattingSettings.AlignAttributesToFirstAttribute = alignAttributesToFirstAttribute.Active;
                XmlFormattingSettings.FirstAttributeOnNewLine = placeFirstAttributeOnNewLine.Active;
                XmlFormattingSettings.AppendSpaceBeforeSlashOnSelfClosingTag = appendSpaceBeforeSlashOnSelfClosingTag.Active;
            }
        }

        WhitespaceMode Mode => (WhitespaceMode)UserOptions.Get(whitespaceModeKey, (int)WhitespaceMode.Spaces);

        int WhitespaceCharacterCount => UserOptions.Get(whitespaceCharacterCountKey, 2);

        string GetWhitespace()
        {
            var count = WhitespaceCharacterCount;

            if (count == 0)
            {
                return string.Empty;
            }

            switch (Mode)
            {
                case WhitespaceMode.Tabs:
                    return new string('t', count);
                case WhitespaceMode.Spaces:
                    return new string(' ', count);
            }

            return string.Empty;
        }
    }
}
