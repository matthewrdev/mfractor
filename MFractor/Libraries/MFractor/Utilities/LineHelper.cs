using System;
using MFractor.Text;
using MFractor.Xml;

namespace MFractor.Utilities
{
    public static class LineHelper
    {
        public static string GetIndent(IXmlNode host, ILine line)
        {
            if (host is null || line is null)
            {
                return string.Empty;
            }

            var indentLength = host.OpeningTagSpan.Start - line.Span.Start;
            if (indentLength <= 0)
            {
                return string.Empty;
            }

            var indent = line.Content.Substring(0, indentLength);
            while (!string.IsNullOrWhiteSpace(indent))
            {
                indent = indent.Substring(0, indent.Length - 1);
            }

            return indent;
        }
    }
}
