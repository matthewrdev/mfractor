using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Editor.Utilities;
using MFractor.Ide;
using MFractor.VS.Mac.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Editor;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IActiveDocument))]
    class ActiveDocument : IActiveDocument
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public ActiveDocument(Lazy<IProjectService> projectService)
        {
            this.projectService = projectService;
        }

        MonoDevelop.Ide.Gui.Document Document => IdeApp.Workbench.ActiveDocument;

        MonoDevelop.Projects.Project DocumentProject => Document?.Owner as MonoDevelop.Projects.Project;

        public FileInfo FileInfo => new FileInfo(Document.FileName);

        public int CaretOffset
        {
            get
            {
                if (!IsAvailable)
                {
                    return 0;
                }

                return Document.GetCaretOffset();
            }
        }

        public Project CompilationProject
        {
            get
            {
                if (Document is null)
                {
                    return default;
                }

                if (DocumentProject is MonoDevelop.Projects.SharedAssetsProjects.SharedAssetsProject sharedAssetsProject)
                {
                }

                if (Document.TextBuffer != null)
                {
                    return TextBufferHelper.GetCompilationProject(Document.TextBuffer);
                }

                return DocumentProject?.ToCompilationProject();
            }
        }

        public OptionSet FormattingOptions => FormattingOptionsForDocument(Document);

        public string FilePath
        {
            get
            {
                if (!IsAvailable)
                {
                    return string.Empty;
                }

                return Document.FilePath;
            }
        }

        public string Name
        {
            get
            {
                if (!IsAvailable)
                {
                    return string.Empty;
                }

                return Path.GetFileName(FilePath);
            }
        }

        public bool IsAvailable => Document != null;

        public Document AnalysisDocument
        {
            get
            {
                if (!IsAvailable)
                {
                    return null;
                }

                return GetAnalysisDocument(Document);
            }
        }

        public IProjectFile ProjectFile
        {
            get
            {
                if (!IsAvailable)
                {
                    return default;
                }

                return ProjectService.GetProjectFileWithFilePath(CompilationProject, FilePath);
            }
        }

        Document GetOpenDocumentInCurrentContext(SourceTextContainer container)
        {
            if (CompilationWorkspace.TryGetWorkspace(container, out var workspace))
            {
                var id = workspace.GetDocumentIdInCurrentContext(container);
                return workspace.CurrentSolution.GetDocument(id);
            }

            return null;
        }

        Document GetAnalysisDocument(MonoDevelop.Ide.Gui.Document ideDocument)
        {
            var textBuffer = ideDocument.GetContent<ITextBuffer>();
            if (textBuffer != null && textBuffer.AsTextContainer() is SourceTextContainer container)
            {
                var document = GetOpenDocumentInCurrentContext(container);
                if (document != null)
                {
                    return document;
                }
            }

            var project = (ideDocument.Owner as MonoDevelop.Projects.Project)?.ToCompilationProject();
            return project.Documents.FirstOrDefault(d => d.FilePath == ideDocument.Name);
        }

        public static OptionSet FormattingOptionsForDocument(MonoDevelop.Ide.Gui.Document document)
        {
            var policyParent = (document?.Owner as MonoDevelop.Projects.Project)?.Policies;
            var types = IdeServices.DesktopService.GetMimeTypeInheritanceChain("text/x-csharp");
            var codePolicy = policyParent != null ? policyParent.Get<MonoDevelop.CSharp.Formatting.CSharpFormattingPolicy>(types) : MonoDevelop.Projects.Policies.PolicyService.GetDefaultPolicy<MonoDevelop.CSharp.Formatting.CSharpFormattingPolicy>(types);
            var textPolicy = policyParent != null ? policyParent.Get<MonoDevelop.Ide.Gui.Content.TextStylePolicy>(types) : MonoDevelop.Projects.Policies.PolicyService.GetDefaultPolicy<MonoDevelop.Ide.Gui.Content.TextStylePolicy>(types);
            return codePolicy.CreateOptions(textPolicy);
        }

        public void SetCaretOffset(int offset)
        {
            if (!IsAvailable)
            {
                return;
            }

            Document.SetCaretLocation(offset);
        }

        public void SetSelection(TextSpan textSpan)
        {
            if (!IsAvailable)
            {
                return;
            }

            Document.SetSelection(textSpan.Start, textSpan.End);
        }

        public FileLocation OffsetToLocation(int offset)
        {
            if (!IsAvailable)
            {
                return null;
            }


            var view = Document.GetContent<ITextView>();

            if (view != null)
            {
                var line = view.TextSnapshot.GetLineFromPosition(offset);
                var column = offset - line.Start;

                return new FileLocation(line.LineNumber, column);
            }

            log?.Warning("The active document " + Document.Name + " does not have an available text editor.");
            return null;

            //var location = Document.edi.OffsetToLocation(offset);

            //return new FileLocation()
            //{
            //    Line = location.Line,
            //    Column = location.Column,
            //};
        }

        public string GetTextBetween(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (!IsAvailable)
            {
                return null;
            }

            var view = Document.GetContent<ITextView>();
            var buffer = Document.GetContent<ITextBuffer>();

            if (buffer != null
                && TryGetSnapshotPoint(view.TextSnapshot, startLine, startColumn, out var startPoint)
                && TryGetSnapshotPoint(view.TextSnapshot, endLine, endColumn, out var endPoint))
            {
                var start = startPoint.Position;
                var end = endPoint.Position;

                return buffer.CurrentSnapshot.GetText(start, end - start);
            }

            
            log?.Warning("The active document " + Document.Name + " does not have an available text editor.");
            return null;

            //return Document.Editor.GetTextBetween(startLine, startColumn, endLine, endColumn);
        }

        public int LocationToOffset(int line, int column)
        {
            if (!IsAvailable)
            {
                return 0;
            }

            var view = Document.GetContent<ITextView>();

            if (view != null
                && TryGetSnapshotPoint(view.TextSnapshot, line, column, out var point))
            {
                return point.Position;
            }

            log?.Warning("The active document " + Document.Name + " does not have an available text editor.");
            return 0;

            //return Document.Editor.LocationToOffset(line, column);
        }

        public InteractionLocation GetInteractionLocation(int? interactionOffset = null)
        {
            if (!IsAvailable)
            {
                return null;
            }

            return Document.GetInteractionLocation(interactionOffset);
        }


        /// <summary>
        /// Gets a snapshot point out of a monodevelop line/column pair or null.
        /// </summary>
        /// <returns>The snapshot point or null if the coordinate is invalid.</returns>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="line">Line number (1 based).</param>
        /// <param name="column">Column number (1 based).</param>
        public SnapshotPoint? GetSnapshotPoint(ITextSnapshot snapshot, int line, int column)
        {
            if (TryGetSnapshotPoint(snapshot, line, column, out var snapshotPoint))
            {
                return snapshotPoint;
            }

            return null;
        }

        /// <summary>
        /// Tries to get a snapshot point out of a monodevelop line/column pair.
        /// </summary>
        /// <returns><c>true</c>, if get snapshot point was set, <c>false</c> otherwise.</returns>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="line">Line number (1 based).</param>
        /// <param name="column">Column number (1 based).</param>
        /// <param name="snapshotPoint">The snapshot point if return == true.</param>
        public bool TryGetSnapshotPoint(ITextSnapshot snapshot, int line, int column, out SnapshotPoint snapshotPoint)
        {
            if (line < 1 || line > snapshot.LineCount)
            {
                snapshotPoint = default(SnapshotPoint);
                return false;
            }

            var lineSegment = snapshot.GetLineFromLineNumber(line - 1);
            if (column < 1 || column > lineSegment.LengthIncludingLineBreak)
            {
                snapshotPoint = default(SnapshotPoint);
                return false;
            }

            snapshotPoint = new SnapshotPoint(snapshot, lineSegment.Start.Position + column - 1);
            return true;
        }
    }
}
