using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MFractor.Xml
{
    /// <summary>
    /// An <see cref="IXmlFormattingPolicy"/> implementation that wraps another policy, using its value as defaults, and allows overriding of individual properties.
    /// </summary>
	public class OverloadableXmlFormattingPolicy : IXmlFormattingPolicy
	{
		readonly IXmlFormattingPolicy wrappedPolicy;

		public OverloadableXmlFormattingPolicy(IXmlFormattingPolicy wrappedPolicy)
		{
			this.wrappedPolicy = wrappedPolicy;
		}

        public string[] MimeTypes => wrappedPolicy.MimeTypes;

        public string Name => "Overloadable Xml Formatting Policy";

        void SetValue<TValue>(TValue value, [CallerMemberName] string key = "")
		{
			OverloadedValues[key] = value;
		}

		TValue GetValue<TValue>(TValue defaultValue, [CallerMemberName] string key = "")
		{
			if (!OverloadedValues.ContainsKey(key))
			{
				return defaultValue;
			}

			return (TValue)OverloadedValues[key];
		}

		protected Dictionary<string, object> OverloadedValues = new Dictionary<string, object>();

		public bool AlignAttributes
        {
            get => GetValue(wrappedPolicy.AlignAttributes);
            set => SetValue(value);
        }

        public bool AlignAttributeValues
        {
            get => GetValue(wrappedPolicy.AlignAttributeValues);
            set => SetValue(value);
        }

        public bool AlignAttributesToFirstAttribute
        {
            get => GetValue(wrappedPolicy.AlignAttributesToFirstAttribute);
            set => SetValue(value);
        }

        public string AttributesIndentString
        {
            get => GetValue(wrappedPolicy.AttributesIndentString);
            set => SetValue(value);
        }

        public bool AttributesInNewLine
        {
            get => GetValue(wrappedPolicy.AttributesInNewLine);
            set => SetValue(value);
        }

        public string ContentIndentString
        {
            get => GetValue(wrappedPolicy.ContentIndentString);
            set => SetValue(value);
        }

        public int EmptyLinesAfterEnd
        {
            get => GetValue(wrappedPolicy.EmptyLinesAfterEnd);
            set => SetValue(value);
        }

        public int EmptyLinesAfterStart
		{
			get
			{
				return GetValue(wrappedPolicy.EmptyLinesAfterStart);
			}
			set
			{
				SetValue(value);
			}
		}

		public int EmptyLinesBeforeEnd
        {
            get => GetValue(wrappedPolicy.EmptyLinesBeforeEnd);
            set => SetValue(value);
        }

        public int EmptyLinesBeforeStart
        {
            get => GetValue(wrappedPolicy.EmptyLinesBeforeStart);
            set => SetValue(value);
        }

        public bool IndentContent
        {
            get => GetValue(wrappedPolicy.IndentContent);
            set => SetValue(value);
        }

        public int MaxAttributesPerLine
        {
            get => GetValue(wrappedPolicy.MaxAttributesPerLine);
            set => SetValue(value);
        }

        public string NewLineChars
        {
            get => GetValue(wrappedPolicy.NewLineChars);
            set => SetValue(value);
        }

        public bool OmitXmlDeclaration
        {
            get => GetValue(wrappedPolicy.OmitXmlDeclaration);
            set => SetValue(value);
        }

        public char QuoteChar
        {
            get => GetValue(wrappedPolicy.QuoteChar);
            set => SetValue(value);
        }

        public int SpacesAfterAssignment
        {
            get => GetValue(wrappedPolicy.SpacesAfterAssignment);
            set => SetValue(value);
        }

        public int SpacesBeforeAssignment
        {
            get => GetValue(wrappedPolicy.SpacesBeforeAssignment);
            set => SetValue(value);
        }

        public bool WrapAttributes
        {
            get => GetValue(wrappedPolicy.WrapAttributes);
            set => SetValue(value);
        }

        public bool FirstAttributeOnNewLine
		{
			get => GetValue(wrappedPolicy.FirstAttributeOnNewLine);
			set => SetValue(value);
		}

        public bool AppendSpaceBeforeSlashOnSelfClosingTag
        {
            get => GetValue(wrappedPolicy.AppendSpaceBeforeSlashOnSelfClosingTag);
            set => SetValue(value);
        }
    }
}
