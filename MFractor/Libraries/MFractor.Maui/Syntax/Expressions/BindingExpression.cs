using System;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	/// <summary>
	/// A design time implementation of the Binding expression.
	/// </summary>
	public class BindingExpression : MarkupExtensionExpression
	{
		public BindingExpression (string name,
								  TextSpan nameSpan,
								  TextSpan expressionSpan,
								  Expression parentExpression,
									XmlAttribute parentAttribute)
			: base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
		{
		}

        public bool ReferencesBindingContext
        {
            get
            {
                if (BindingValue == null)
                {
                    return false;
                }

                if (BindingValue.HasValue)
                {
                    return BindingValue.Value.Trim() == ".";
                }

                return false;
            }
        }

        public LiteralValueExpression BindingValue => GetChildExpression<LiteralValueExpression>();

        public PathExpression Path => GetChildExpression<PathExpression>();

        public BindingModeExpression BindingMode => GetChildExpression<BindingModeExpression>();

        public ConverterExpression Converter => GetChildExpression<ConverterExpression>();

        public ConverterParameterExpression ConverterParameter => GetChildExpression<ConverterParameterExpression>();

        public StringFormatExpression StringFormat => GetChildExpression<StringFormatExpression>();

        public SourceExpression Source => GetChildExpression<SourceExpression>();

        public bool HasReferencedSymbol
		{
			get
			{
				if (Path != null)
				{
                    return Path.AssignmentValue != null;
				}

				if (BindingValue != null)
				{
					return BindingValue.HasValue;
				}

				return false;
			}
		}

		public string ReferencedSymbolValue
		{
			get
			{
				if (Path != null)
				{
					return Path.AssignmentValue.ToString().Trim();
				}

				if (BindingValue != null)
				{
					return BindingValue.Value.Trim();
				}

				return "";
			}
		}

		public TextSpan ReferencedSymbolSpan
		{
			get
			{
				if (Path != null)
				{
					return Path.AssignmentValue.Span;
				}

				if (BindingValue != null)
				{
					return BindingValue.Span;
				}

                return default(TextSpan);
			}
		}

		public override string ToString()
		{
			string content = this.MarkupExtension + " ";

			if (BindingValue != null && BindingValue.HasValue)
			{
				content += BindingValue.Value + ", ";
			}

			if (Source != null)
			{
				content += Source.ToString() + ", ";
			}

			if (Path != null)
			{
				content += Path.ToString() + ", ";
			}

			if (StringFormat != null)
			{
				content += StringFormat.ToString() + ", ";
			}

			if (Converter != null)
			{
				content += Converter.ToString() + ", ";
			}

			if (ConverterParameter != null)
			{
				content += ConverterParameter.ToString() + ", ";
			}

			if (BindingMode != null)
			{
				content += BindingMode.ToString() + ", ";
			}

			return content;
		}
	}
}

