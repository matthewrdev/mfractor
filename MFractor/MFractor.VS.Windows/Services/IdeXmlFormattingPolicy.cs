using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFractor.Xml;

namespace MFractor.VS.Windows.Services
{
    class IdeXmlFormattingPolicy : IXmlFormattingPolicy
    {

        public IdeXmlFormattingPolicy()
        {

        }

        public bool AlignAttributes => throw new NotImplementedException();

        public bool AlignAttributeValues => throw new NotImplementedException();

        public string AttributesIndentString => throw new NotImplementedException();

        public bool AlignAttributesToFirstAttribute => throw new NotImplementedException();

        public bool AttributesInNewLine => throw new NotImplementedException();

        public string ContentIndentString => throw new NotImplementedException();

        public int EmptyLinesAfterEnd => throw new NotImplementedException();

        public int EmptyLinesAfterStart => throw new NotImplementedException();

        public int EmptyLinesBeforeEnd => throw new NotImplementedException();

        public int EmptyLinesBeforeStart => throw new NotImplementedException();

        public bool IndentContent => throw new NotImplementedException();

        public int MaxAttributesPerLine => throw new NotImplementedException();

        public string NewLineChars => throw new NotImplementedException();

        public bool OmitXmlDeclaration => throw new NotImplementedException();

        public char QuoteChar => throw new NotImplementedException();

        public int SpacesAfterAssignment => throw new NotImplementedException();

        public int SpacesBeforeAssignment => throw new NotImplementedException();

        public bool WrapAttributes => throw new NotImplementedException();

        public bool FirstAttributeOnNewLine => throw new NotImplementedException();

        public bool AppendSpaceBeforeSlashOnSelfClosingTag => throw new NotImplementedException();

        public string[] MimeTypes => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();
    }
}
