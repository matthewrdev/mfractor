using System;using System.ComponentModel.Composition;using System.IO;using System.Linq;using EnvDTE;
using EnvDTE80;using MFractor.Code.Formatting;using MFractor.Ide;
using MFractor.IOC;
using MFractor.VS.Windows.Utilities;using MFractor.VS.Windows.WorkspaceModel;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;using Microsoft.CodeAnalysis.Options;using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace MFractor.VS.Windows.Services{    [PartCreationPolicy(CreationPolicy.Shared)]    [Export(typeof(IActiveDocument))]    class ActiveDocument : IActiveDocument    {
        readonly Logging.ILogger log = Logging.Logger.Create();        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IWorkspaceShadowModel> workspaceShadowModel;
        IWorkspaceShadowModel WorkspaceShadowModel => workspaceShadowModel.Value;        [ImportingConstructor]        public ActiveDocument(Lazy<ICodeFormattingPolicyService> formattingPolicyService,                              Lazy<IWorkspaceShadowModel> workspaceShadowModel)        {            this.formattingPolicyService = formattingPolicyService;
            this.workspaceShadowModel = workspaceShadowModel;
        }

        private DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;


        // TODO: This needs to be converted to an ITextView or ITextBuffer.
        EnvDTE.Document Document
        {
            get
            {
                if (!ThreadHelper.CheckAccess())
                {
                    log?.Warning("ActiveDocument.Document is being call from a non ui thread");
                    return default;
                }

                if (DTE.ActiveWindow == null)
                {
                    return default;
                }

                if (DTE.ActiveWindow.Document == null)
                {
                    return default;
                }

                try
                {
                    return DTE.ActiveDocument;
                }
                catch (Exception ex)
                {

                }

                return default;
            }
        }

        public FileInfo FileInfo        {            get            {                if (!IsAvailable)                {                    return default;                }

                return new FileInfo(FilePath);            }        }        public int CaretOffset        {            get            {                if (!IsAvailable)                {                    return 0;                }                // TODO: Need to convert this to ITextView?                var selection = Document.Selection as TextSelection;                if (selection == null || selection.ActivePoint == null)                {                    return 0;                }                return selection.ActivePoint.AbsoluteCharOffset;            }        }        public Microsoft.CodeAnalysis.Project CompilationProject => ProjectFile?.CompilationProject;        public OptionSet FormattingOptions => FormattingPolicyService.GetFormattingPolicy(CompilationProject).OptionSet;        public string Name => ProjectFile?.Name;

        public IdeProject IdeProject
        {
            get
            {
                if (!IsAvailable)
                {
                    return default;
                }

                var guid = DteProjectHelper.GetProjectGuid(Document.ProjectItem.ContainingProject);

                return WorkspaceShadowModel.GetProjectByGuid(guid);
            }
        }

        public bool IsAvailable => Document != null && Document.ProjectItem != null;

        public Microsoft.CodeAnalysis.Document AnalysisDocument        {            get            {                if (!IsAvailable)                {                    return null;                }                var project = CompilationProject;                if (project == null)                {                    return default;                }                return project.Documents.FirstOrDefault(d => d.FilePath == Document.FullName);            }        }

        ITextBuffer GetBufferAt(string filePath)
        {
            var view = GetTextView(filePath);

            if (view == null)
            {
                return default;
            }

            if (view.GetBuffer(out var lines) == 0)
            {
                if (lines is IVsTextBuffer buffer)
            {
                var editorAdapterFactoryService = Resolver.Resolve<IVsEditorAdaptersFactoryService>();
                return editorAdapterFactoryService.GetDataBuffer(buffer);
                }
            }

            return null;
        }

        IVsTextView GetTextView(string filePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var sp = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)DTE;
            var serviceProvider = new ServiceProvider(sp);

            if (VsShellUtilities.IsDocumentOpen(serviceProvider, filePath, Guid.Empty, out var hierarchy, out var itemId, out var windowFrame))
            {
                var view = VsShellUtilities.GetTextView(windowFrame);
                return view;
            }

            return null;
        }        public string FilePath => Document?.FullName;

        public void SetCaretOffset(int offset)        {            if (!IsAvailable)            {                return;            }

            var view = GetTextView(FilePath);            if (view == null)
            {
                return;
            }            var location = OffsetToLocation(offset);            view.SetCaretPos(location.Line, location.Column);        }        public void SetSelection(Microsoft.CodeAnalysis.Text.TextSpan textSpan)        {            if (!IsAvailable)            {                return;            }            var view = GetTextView(FilePath);            if (view == null)
            {
                return;
            }            var star = OffsetToLocation(textSpan.Start);            var end = OffsetToLocation(textSpan.End);            view.SetSelection(star.Line, star.Column, end.Line, end.Column);        }        public FileLocation OffsetToLocation(int offset)        {            if (!IsAvailable)            {                return null;            }

            var view = GetTextView(FilePath);            if (view == null)
            {                return null;
            }            view.GetLineAndColumn(offset, out var line, out var column);            return new FileLocation()            {                Line = line,                Column = column,            };        }        public string GetTextBetween(int startLine, int startColumn, int endLine, int endColumn)        {            if (!IsAvailable)            {                return null;            }

            var view = GetBufferAt(FilePath);            if (view == null)
            {                return null;
            }

            var start = LocationToOffset(startLine, startColumn);
            var end = LocationToOffset(endLine, endColumn);

            return view.CurrentSnapshot.GetText(start, end - start);        }        public int LocationToOffset(int line, int column)        {            if (!IsAvailable)            {                return 0;            }

            var view = GetBufferAt(FilePath);            if (view == null)
            {                return 0;            }            if (line == 0)
            {
                return column;
            }            if (line > view.CurrentSnapshot.LineCount)
            {
                log?.Warning("Attemping to access a line that is longer than the buffers length");
                return view.CurrentSnapshot.Length; ;
            }            var currentOffset = 0;            foreach (var snapshotLine in view.CurrentSnapshot.Lines.Take(line))
            {
                currentOffset += snapshotLine.Length + snapshotLine.LineBreakLength;
            }            currentOffset += column;

            return currentOffset;        }        public InteractionLocation GetInteractionLocation(int? interactionOffset = null)        {            if (!IsAvailable)            {                return null;            }            var selection = Document.Selection as TextSelection;            if (selection == null || selection.ActivePoint == null)            {                return null;            }            // TODO: For the current document, get the interaction information (caret offset and active selection).            return default;        }

        public IProjectFile ProjectFile
        {
            get
            {
                if (!IsAvailable)
                {
                    return default;
                }

                ThreadHelper.ThrowIfNotOnUIThread();

                var project = IdeProject;
                if (project == null)
                {
                    return default;
                }

                var filePath = DteProjectHelper.GetProjectItemFilePath(Document.ProjectItem);

                return project.GetProjectFile(filePath);
            }
        }
    }}