using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MFractor.Xml;

namespace MFractor.Editor.XAML.Services
{
    class XmlSyntaxTreeCache
    {
        readonly Dictionary<string, XmlSyntaxTree> syntaxTreeTable = new Dictionary<string, XmlSyntaxTree>();
        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;

        public XmlSyntaxTreeCache(Lazy<IXmlSyntaxParser> xmlSyntaxParser)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
        }

        XmlSyntaxTree Add(string fileName)
        {
            if (!IsXml(fileName))
            {
                return null;
            }

            var content = File.ReadAllText(fileName);
            var ast = xmlSyntaxParser.Value.ParseText(content);

            lock (syntaxTreeTable)
            {
                syntaxTreeTable[fileName] = ast;
            }

            return ast;
        }

        public void Set(string fileName)
        {
            Task.Run(() => Add(fileName));
        }

        public XmlSyntaxTree Get(string fileName, bool isCached)
        {
            if (isCached)
            {
                lock (syntaxTreeTable)
                {
                    if (syntaxTreeTable.TryGetValue(fileName, out var value))
                    {
                        return value;
                    }
                }
            }

            return Add(fileName);
        }

        public void Remove(string fileName)
        {
            if (!IsXml(fileName))
            {
                return;
            }

            lock (syntaxTreeTable)
            {
                if (syntaxTreeTable.ContainsKey(fileName))
                {
                    syntaxTreeTable.Remove(fileName);
                }
            }
        }

        bool IsXml(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return extension == ".xml" || extension == ".xaml";
        }
    }
}
