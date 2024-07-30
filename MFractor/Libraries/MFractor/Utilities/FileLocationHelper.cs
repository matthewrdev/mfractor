using System;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for performing file location checks.
    /// </summary>
    public static class FileLocationHelper
	{
		public static bool IsBetween(int position, TextSpan span)
		{
            return span.IntersectsWith(position);
		}

		public static bool IsBetween(int position, int start, int end)
		{
            return unchecked((uint)(position - start) <= (uint)(end - start));
		}

        public static bool IsBetween(int position, Xml.XmlNode element)
        {
            var span = element.IsSelfClosing ? element.OpeningTagSpan : element.Span;

            return span.IntersectsWith(position);
        }

        public static bool IsBetween(int position, Xml.XmlAttribute attribute)
		{
            var span = attribute.Span;

			return span.IntersectsWith(position);
        }

        public static bool IsBetween(int line,
                                     int column,
                                     int startLine,
                                     int startColumn,
                                     int endLine,
                                     int endColumn)
        {
            if (line < startLine)
            {
                return false;
            }

            if (line > endLine)
            {
                return false;
            }

            if (line == endLine)
            {
                return column >= startColumn && column <= endColumn;
            }

            if (line == startLine)
            {
                return column >= startColumn;
            }

            if (line == endLine)
            {
                return column <= endColumn;
            }

            return true;
        }
    }
}
