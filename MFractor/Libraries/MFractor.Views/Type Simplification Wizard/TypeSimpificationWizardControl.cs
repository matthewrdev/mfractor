using System;
using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.IOC;
using MFractor.Text;
using MFractor.Workspace;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.TypeSimplificationWizard
{
    public class TypeSimpificationWizardControl : Xwt.VBox
    {
        public TypeSimpificationWizardControl()
        {
            Resolver.ComposeParts(this);

            Build();
        }

        HBox textEditorContainer;
        ITextEditor beforeEditor;
        Label titleLabel;
        ITextEditor afterEditor;
        Button applyButton;
        IProjectFile projectFile;

        [Import]
        public ICSharpSyntaxReducer SyntaxReducer { get; set; }

        [Import]
        public ITextEditorFactory TextEditorFactory { get; set; }

        [Import]
        public ITextProviderService TextProviderService { get; set; }

        public event EventHandler<TypeSimplificationEventArgs> TypeSimplicationConfirmed;

        void Build()
        {
            // Title of file

            titleLabel = new Label()
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Bold)
            };
            PackStart(titleLabel);
            PackStart(new HSeparator());

            textEditorContainer = new HBox();

            beforeEditor = TextEditorFactory.Create();
            beforeEditor.MimeType = LanguageMimeTypes.CSharp;
            textEditorContainer.PackStart(beforeEditor.Widget, true, true);

            textEditorContainer.PackStart(new VSeparator());

            afterEditor = TextEditorFactory.Create();
            afterEditor.MimeType = LanguageMimeTypes.CSharp;
            textEditorContainer.PackStart(afterEditor.Widget, true, true);

            PackStart(textEditorContainer, true, true);

            PackStart(new HSeparator());

            applyButton = new Button()
            {
                Label = "Apply"
            };
            applyButton.Clicked += ApplyButton_Clicked;
            PackStart(applyButton);

            PackStart(new HSeparator());
            PackStart(new Branding.BrandedFooter("https://docs.mfractor.com/csharp/code-actions/simplified-qualified-types/", "Simplify Qualified Types"));
        }

        void ApplyButton_Clicked(object sender, EventArgs e)
        {
            TypeSimplicationConfirmed?.Invoke(this, new TypeSimplificationEventArgs(projectFile, afterEditor.Text));
        }

        public void SetProjectFile(IProjectFile projectFile)
        {
            this.projectFile = projectFile;
            titleLabel.Text = projectFile.VirtualPath;

            var textProvider = TextProviderService.GetTextProvider(projectFile.FilePath);

            var content = textProvider.GetText();

            beforeEditor.Text = content;
            afterEditor.Text = SyntaxReducer.Reduce(content, projectFile.CompilationProject);
        }
    }
}
