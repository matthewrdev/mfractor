using System;
using MFractor.Editor.Utilities;
using MFractor.IOC;
using MFractor.Workspace;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor
{
    class TextViewBinding : IDisposable
    {
        public ITextView TextView { get; }
        string projectGuid;
        readonly string filePath;

        readonly IMutableWorkspaceService mutableWorkspaceService;
        readonly IMutableTextViewService mutableTextViewService;

        public TextViewBinding(string filePath,
                               string projectGuid,
                               ITextView textView,
                               IMutableWorkspaceService mutableWorkspaceService,
                               IMutableTextViewService mutableTextViewService)
        {
            this.TextView = textView;
            this.filePath = filePath;
            this.projectGuid = projectGuid;
            this.mutableWorkspaceService = mutableWorkspaceService;
            this.mutableTextViewService = mutableTextViewService;

            BindEvents();
        }

        void BindEvents()
        {
            UnbindEvents();

            TextView.Closed += TextView_Closed;
            TextView.TextBuffer.Changed += TextBuffer_Changed;
        }

        void UnbindEvents()
        {
            TextView.Closed -= TextView_Closed;
            TextView.TextBuffer.Changed -= TextBuffer_Changed;
        }

        void TextBuffer_Changed(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                var projectService = Resolver.Resolve<IProjectService>();

               var project =  TextBufferHelper.GetCompilationProject(this.TextView.TextBuffer);

                projectGuid = projectService.GetProjectGuid(project);
            }

            mutableWorkspaceService.NotifyFileChanged(projectGuid, filePath);
        }

        void TextView_Closed(object sender, EventArgs e)
        {
            UnbindEvents();

            mutableTextViewService.NotifyClosed(filePath, TextView);
        }

        public void Dispose()
        {
            UnbindEvents();
        }
    }
}
