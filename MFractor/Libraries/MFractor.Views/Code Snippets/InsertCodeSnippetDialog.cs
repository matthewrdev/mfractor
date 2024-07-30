using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.CodeSnippets;
using MFractor.IOC;
using MFractor.Views.Branding;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.CodeSnippets
{
    public class InsertCodeSnippetDialog : Dialog
    {
        VBox root;
        HBox mainContainer;
        ScrollView codeArgumentsScrollView;

        public ICodeSnippet CodeSnippet { get; }
        public string HelpUrl { get; }
        public string MimeType { get; }

        IReadOnlyList<CodeArgumentControl> codeArguments;

        VBox codeArgumentsContainer;

        ITextEditor textEditor;
        Button generateButton;

        public event EventHandler<InsertCodeSnippetEventArgs> OnInsertCodeSnippet;

        readonly string featureName;
        readonly Func<string, ICodeSnippet, bool> onArgumentValueEditedFunc;

        public InsertCodeSnippetDialog(ICodeSnippet codeSnippet,
                                       string helpUrl,
                                       string featureName,
                                       Func<string, ICodeSnippet, bool> onArgumentValueEditedFunc = null,
                                       string title = "Insert Code Snippet",
                                       string buttonText = "Insert Code Snippet",
                                       string mimeType = "text/x-csharp")
        {
            Width = 960;
            Height = 720;

            CodeSnippet = codeSnippet;
            HelpUrl = helpUrl;
            this.featureName = featureName;
            this.onArgumentValueEditedFunc = onArgumentValueEditedFunc;
            MimeType = mimeType;

            Title = title;
            Icon = Image.FromResource("mfractor_logo.png");

            Build();

            generateButton.Label = buttonText;

            ApplyText();
        }

        void Build()
        {
            root = new VBox();

            mainContainer = new HBox();

            var args = new List<CodeArgumentControl>();

            codeArgumentsContainer = new VBox();

            foreach (var arg in CodeSnippet.Arguments)
            {
                var control = new CodeArgumentControl(arg);
                control.ValueChanged += Control_ValueChanged;
                args.Add(control);
                codeArgumentsContainer.PackStart(control);
            }

            codeArguments = args;

            codeArgumentsScrollView = new ScrollView()
            {
                WidthRequest = 300,
            };
            codeArgumentsScrollView.Content = codeArgumentsContainer;
            mainContainer.PackStart(codeArgumentsScrollView);

            textEditor = Resolver.Resolve<ITextEditorFactory>().Create();
            textEditor.MimeType = MimeType;
            mainContainer.PackStart(textEditor.Widget, true, true);

            root.PackStart(mainContainer, true, true);

            generateButton = new Button("Insert Code Snippet");
            generateButton.Clicked += Button_Clicked;

            root.PackStart(generateButton);
            root.PackStart(new HSeparator());
            root.PackStart(new BrandedFooter(HelpUrl, featureName));

            Content = root;

            if (codeArguments.Any())
            {
                codeArguments.First().SetFocus();
            }
        }

        void Button_Clicked(object sender, EventArgs e)
        {
            OnInsertCodeSnippet?.Invoke(this, new InsertCodeSnippetEventArgs(CodeSnippet));

            this.Close();
            this.Dispose();
        }

        void Control_ValueChanged(object sender, EventArgs e)
        {
            ApplyText();

            if (onArgumentValueEditedFunc != null)
            {
                var codeControl = sender as CodeArgumentControl;

                var argument = codeControl.CodeSnippetArgument;

                if (onArgumentValueEditedFunc.Invoke(argument.Name, CodeSnippet))
                {
                    foreach (var control in codeArguments)
                    {
                        control.Sync();
                    }
                }
            }
        }

        void ApplyText()
        {
            textEditor.Text = CodeSnippet.ToString();
        }
    }
}
