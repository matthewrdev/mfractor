using System.Diagnostics;

namespace MFractor.Maui.Styles
{
    [DebuggerDisplay("{Value}")]
    class LiteralStylePropertyValue : ILiteralStylePropertyValue
    {
        public LiteralStylePropertyValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}
