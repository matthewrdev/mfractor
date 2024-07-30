using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Text;
using MFractor.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXmlSyntaxTreeService))]
    class XmlSyntaxTreeService : IXmlSyntaxTreeService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        class TextViewBinding : IDisposable
        {
            readonly ITextView textView;
            readonly string filePath;
            readonly XmlSyntaxTreeService parent;

            public TextViewBinding(string filePath, ITextView textView, XmlSyntaxTreeService parent)
            {
                this.textView = textView;
                this.filePath = filePath;
                this.parent = parent;

                BindEvents();
            }

            void BindEvents()
            {
                UnbindEvents();

                textView.Closed += TextView_Closed;
                textView.TextBuffer.Changed += TextBuffer_Changed;
            }

            void UnbindEvents()
            {
                textView.Closed -= TextView_Closed;
                textView.TextBuffer.Changed -= TextBuffer_Changed;
            }

            void TextBuffer_Changed(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
            {
                var changes = e.Changes.Select(change => new MFractor.Text.TextReplacement()
                {
                    Offset = change.OldPosition,
                    Length = change.OldLength,
                    Text = change.NewText
                }).ToList();

                parent.OnDocumentContentChanged(filePath, e.After.TextBuffer, changes);
            }

            void TextView_Closed(object sender, EventArgs e)
            {
                UnbindEvents();

                parent.OnDomentClosed(filePath);
            }

            public void Dispose()
            {
                UnbindEvents();
            }
        }

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        [ImportingConstructor]
        public XmlSyntaxTreeService(Lazy<IXmlSyntaxParser> xmlSyntaxParser)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
        }

        readonly Dictionary<string, XmlSyntaxTree> syntaxTreeTable = new Dictionary<string, XmlSyntaxTree>();
        readonly Dictionary<string, TextViewBinding> textViewBindings = new Dictionary<string, TextViewBinding>();

        public event EventHandler<XmlSyntaxTreeEventArgs> SyntaxTreeUpdated;
        public event EventHandler<XmlSyntaxTreeEventArgs> SyntaxTreeRemoved;

        public XmlSyntaxTree GetSyntaxTree(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                log?.Warning("A null or empty file name was used to try get a cached xml syntax tree.");
                return default;
            }

            lock (syntaxTreeTable)
            {
                if (syntaxTreeTable.TryGetValue(filePath, out var value))
                {
                    return value;
                }
            }

            return default;
        }

        public void BindTextView(string filePath, ITextView textView)
        {
            var binding = new TextViewBinding(filePath, textView, this);

            textViewBindings[filePath] = binding;

            var text = textView.TextBuffer.CurrentSnapshot.GetText();
            ParseSyntaxTree(filePath, text);
        }

        void OnDocumentContentChanged(string filePath, ITextBuffer textBuffer, IEnumerable<ITextReplacement> changes)
        {
            if (!IsXml(filePath))
            {
                return;
            }

            var text = textBuffer.CurrentSnapshot.GetText();

            ParseSyntaxTreeIncremental(filePath, text, changes);
        }

        void ParseSyntaxTree(string filePath, string text)
        {
            var ast = XmlSyntaxParser.ParseText(text);
            lock (syntaxTreeTable)
            {
                syntaxTreeTable[filePath] = ast;
            }

            SyntaxTreeUpdated?.Invoke(this, new XmlSyntaxTreeEventArgs(ast, filePath));
        }

        void ParseSyntaxTreeIncremental(string filePath, string text, IEnumerable<ITextReplacement> changes)
        {
            var oldAst = syntaxTreeTable[filePath];
            XmlSyntaxTree ast = null;
            try
            {
                ast = XmlSyntaxParser.ParseTextIncremental(text, oldAst, changes);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            lock (syntaxTreeTable)
            {
                syntaxTreeTable[filePath] = ast;
            }

            SyntaxTreeUpdated?.Invoke(this, new XmlSyntaxTreeEventArgs(ast, filePath));
        }

        void OnDomentClosed(string filePath)
        {
            if (!IsXml(filePath))
            {
                return;
            }

            lock (syntaxTreeTable)
            {
                if (syntaxTreeTable.ContainsKey(filePath))
                {
                    syntaxTreeTable.Remove(filePath);
                }
            }

            SyntaxTreeRemoved?.Invoke(this, new XmlSyntaxTreeEventArgs(null, filePath));

            if (textViewBindings.ContainsKey(filePath))
            {
                textViewBindings.Remove(filePath);
            }
        }

        bool IsXml(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var extension = Path.GetExtension(filePath);

            return extension == ".xml" || extension == ".xaml";
        }
    }
}
