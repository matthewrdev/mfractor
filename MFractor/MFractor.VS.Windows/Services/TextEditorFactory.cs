using System;
using System.ComponentModel.Composition;
using MFractor.Views;
using Xwt;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ITextEditorFactory))]
    class TextEditorFactory : ITextEditorFactory
    {
        public ITextEditor Create()
        {
            return new TextEditorImpl();
        }

        class TextEditorImpl : ITextEditor
        {
            readonly Xwt.ScrollView editorContainer;

            readonly Xwt.TextEntry editor;

            public Widget Widget => editorContainer;

            public event EventHandler<EventArgs> SelectionChanged;

            public string Text
            {
                get => editor.Text;
                set => editor.Text = value;
            }

            public string MimeType
            {
                get;
                set;
            }

            public bool IsReadOnly
            {
                get => !editor.Sensitive;
                set => editor.Sensitive = !value;
            }

            public TextEditorImpl()
            {
                editor = new TextEntry();
                editor.MultiLine = true;
                editor.SelectionChanged += (sender, e) =>
                {
                    SelectionChanged?.Invoke(this, e);
                };

                editorContainer = new ScrollView();
                editorContainer.Content = editor;
                editorContainer.VerticalScrollPolicy = ScrollPolicy.Always;
            }

            public void ClearSelection()
            {
                editor.SelectionLength = 0;
            }

            public void Dispose()
            {
                editor.Dispose();
            }
        }
    }
}
