using System;
using System.Collections.Generic;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
    // TODO: Break this into a NamespaceExpression, PropertyExpression, NamedTypeExpression
	public class DotNetTypeSymbolExpression : ValueExpression
	{
		public DotNetTypeSymbolExpression(string value,
                                          TextSpan span,
		                              Expression parentExpression,
									  XmlAttribute parentAttribute,
                                      bool isMarkupExtension = false)
			: base(span, parentExpression, parentAttribute)
		{
			this.Value = value;

			if (!string.IsNullOrEmpty(value))
			{
				if (value.Contains(":")) {
					var components = value.Split(':');

                    var namespaceStart = span.Start;
					int namespaceEnd = namespaceStart;
					if (components.Length > 0)
					{
						// Inject the namespace region
						string content = "";
						var namespaceComponent = components[0];
						for (int i = 0; i < namespaceComponent.Length; ++i)
						{
							var c = namespaceComponent[i];
                            namespaceEnd++;

							content += c;
						}

						this.Namespace = content;
                        this.NamespaceSpan = TextSpan.FromBounds(namespaceStart, namespaceEnd);
					}

					if (components.Length > 1)
					{
                        var symbolStart = namespaceEnd + 1; // + 1 to skip the : character.
                        var symbolEnd = symbolStart;

						string content = "";
						var symbolReference = value.Remove(0, components[0].Length + 1);
						for (int i = 0; i < symbolReference.Length; ++i)
						{
							var c = symbolReference[i];
                            symbolEnd++;

							content += c;
						}

						Symbol = content;
                        SymbolSpan = TextSpan.FromBounds(symbolStart, symbolEnd);
						_className = content;

						if (Symbol.Contains("."))
						{
							var symbolComponents = Symbol.Split('.');

							if (symbolComponents.Length > 0)
							{
								_className = symbolComponents[0];
							}

							if (symbolComponents.Length > 1)
							{
								_propertyName = symbolComponents[1];
							}
						}
					}
				}
				else {
					Symbol = this.Value;
					SymbolSpan = span;
				}
			}

            IsMarkupExtension = isMarkupExtension;
        }

		public string Namespace
		{
			get;
			private set;
		}

        public bool HasNamespace => string.IsNullOrEmpty(Namespace) == false;

        public TextSpan NamespaceSpan
		{
			get;
			private set;
		}

		public string Symbol
		{
			get;
			private set;
		}

        public bool HasSymbol => string.IsNullOrEmpty(Symbol) == false;

        private string _className;
        public string ClassName => _className;

        public bool HasClassComponent => !string.IsNullOrEmpty(_className);

        private string _propertyName;
        public string PropertyName => _propertyName;

        public bool HasPropertyComponent => !string.IsNullOrEmpty(_propertyName);

        public TextSpan SymbolSpan
		{
			get;
			private set;
		}
        public bool IsMarkupExtension { get; }

        public override string ToString()
		{
			string content = "";
			if (HasNamespace)
			{
				content += Namespace + ":";
			}

			if (HasSymbol)
			{
				content += Symbol;
			}

			return content;
		}
	}
}

