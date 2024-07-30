using System;
using MFractor.CodeSnippets;
using MFractor.Utilities;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.CodeSnippets
{
    public class CodeArgumentControl : VBox
    {
        Label title;
        TextEntry entry;
        Button clearButton;

        HBox entryContainer;

        public event EventHandler ValueChanged;

        public CodeArgumentControl(ICodeSnippetArgument codeSnippetArgument)
        {
            CodeSnippetArgument = codeSnippetArgument;

            title = new Label(codeSnippetArgument.Name.Replace("_", " ").WordStartingCharactersToUpper());
            entry = new TextEntry();

            entryContainer = new HBox();

            entry.Text = CodeSnippetArgument.Value;
            entry.TooltipText = CodeSnippetArgument.Description;
            entry.PlaceholderText = CodeSnippetArgument.Description;
            entry.Changed += Entry_Changed;

            entryContainer.PackStart(entry, true, true);

            clearButton = new Button(Image.FromResource("cross.png").WithSize(10, 16));
            clearButton.Clicked += ClearButton_Clicked;

            entryContainer.PackStart(clearButton);

            PackStart(title);
            PackStart(entryContainer);
            PackStart(new HSeparator());
        }

        private void ClearButton_Clicked(object sender, EventArgs e)
        {
            entry.Text = string.Empty;
        }

        void Entry_Changed(object sender, EventArgs e)
        {
            CodeSnippetArgument.Value = entry.Text;
            clearButton.Visible = !string.IsNullOrEmpty(entry.Text);
            ValueChanged?.Invoke(this, e);
        }

        public ICodeSnippetArgument CodeSnippetArgument { get; }

        public string Value
        {
            get => entry.Text;
            set => entry.Text = value;
        }

        protected override void OnGotFocus(EventArgs args)
        {
            base.OnGotFocus(args);

            entry.SetFocus();
        }

        internal void Sync()
        {
            if (entry.Text != CodeSnippetArgument.Value)
            {
                entry.Text = CodeSnippetArgument.Value;
            }
        }
    }
}
