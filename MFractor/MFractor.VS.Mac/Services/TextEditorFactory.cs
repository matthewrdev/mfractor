using System;
using System.ComponentModel.Composition;
using MFractor.Views;
using MonoDevelop.Ide.Editor;
using Xwt;

namespace MFractor.VS.Mac.Services
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
            readonly Xwt.TextEntry editor;

            public Widget Widget => editor;

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
                get => editor.Sensitive == false;
                set => editor.Sensitive = !value;
            }

            public bool AllowSelection { get; set; }

            void OnSelectionChanged(object sender, EventArgs e)
            {
                if (!AllowSelection)
                {
                    try
                    {
                        editor.SelectionChanged -= OnSelectionChanged;
                        ClearSelection();
                    }
                    finally
                    {
                        editor.SelectionChanged += OnSelectionChanged;
                    }
                    return;
                }

                SelectionChanged?.Invoke(this, e);
            }

            public event EventHandler<EventArgs> SelectionChanged;

            public TextEditorImpl()
            {
                editor = new TextEntry()
                {
                    MultiLine = true,
                    ExpandVertical = true,
                };
                editor.SelectionChanged += OnSelectionChanged;

                AllowSelection = false;
                IsReadOnly = true;
            }

            public void ClearSelection()
            {
                //editor.ClearSelection();
            }

            public void Dispose()
            {
                editor.SelectionChanged -= OnSelectionChanged;
                editor.Dispose();
            }
        }
    }
}
