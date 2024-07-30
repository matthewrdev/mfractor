﻿using System;
using EnvDTE80;
using MFractor.IOC;
using MFractor.VS.Windows.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace MFractor.VS.Windows.Services
        readonly Logging.ILogger log = Logging.Logger.Create();
        ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IWorkspaceShadowModel> workspaceShadowModel;
        IWorkspaceShadowModel WorkspaceShadowModel => workspaceShadowModel.Value;
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

        public FileInfo FileInfo

                return new FileInfo(FilePath);

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

        public Microsoft.CodeAnalysis.Document AnalysisDocument

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
        }

        public void SetCaretOffset(int offset)

            var view = GetTextView(FilePath);
            {
                return;
            }
            {
                return;
            }

            var view = GetTextView(FilePath);
            {
            }

            var view = GetBufferAt(FilePath);
            {
            }

            var start = LocationToOffset(startLine, startColumn);
            var end = LocationToOffset(endLine, endColumn);

            return view.CurrentSnapshot.GetText(start, end - start);

            var view = GetBufferAt(FilePath);
            {
            {
                return column;
            }
            {
                log?.Warning("Attemping to access a line that is longer than the buffers length");
                return view.CurrentSnapshot.Length; ;
            }
            {
                currentOffset += snapshotLine.Length + snapshotLine.LineBreakLength;
            }

            return currentOffset;

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
    }