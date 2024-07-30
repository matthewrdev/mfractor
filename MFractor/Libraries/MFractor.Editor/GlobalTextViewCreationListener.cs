using System;
using System.ComponentModel.Composition;
using MFractor.Editor.Utilities;
using MFractor.Workspace;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor
{
    [Export(typeof(ITextViewCreationListener))]
    [ContentType(ContentTypes.Any)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    class GlobalTextViewCreationListener : ITextViewCreationListener
    {
        readonly Lazy<IMutableTextViewService> textViewService;
        public IMutableTextViewService TextViewService => textViewService.Value;

        readonly Lazy<ITextDocumentFactoryService> textDocumentFactory;
        public ITextDocumentFactoryService TextDocumentFactory => textDocumentFactory.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public GlobalTextViewCreationListener(Lazy<IMutableTextViewService> textViewService,
                                              Lazy<ITextDocumentFactoryService> textDocumentFactory,
                                              Lazy<IProjectService> projectService)
        {
            this.textViewService = textViewService;
            this.textDocumentFactory = textDocumentFactory;
            this.projectService = projectService;
        }
        
        public void TextViewCreated(ITextView textView)
        {
            if (!TextDocumentFactory.TryGetTextDocument(textView.TextBuffer, out var textDocument))
            {
                return;
            }

            var project = TextBufferHelper.GetCompilationProject(textView.TextBuffer);

            var projectGuid = ProjectService.GetProjectGuid(project);

            TextViewService.BindTextView(textDocument.FilePath, projectGuid, textView);

            TextViewService.NotifyOpened(textDocument.FilePath, textView);
        }
    }
}
