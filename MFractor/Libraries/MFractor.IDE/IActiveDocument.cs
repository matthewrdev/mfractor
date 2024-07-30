using System.IO;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Ide
{
    /// <summary>
    /// The currently opened and editing document in the users workspace.
    /// </summary>
    public interface IActiveDocument
    {
        /// <summary>
        /// The project that this document is owned by.
        /// <para/>
        /// May be nul
        /// </summary>
        Project CompilationProject { get; }

        /// <summary>
        /// The file path of this document.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// The name of this document, excluding the file path.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The file information of this document.
        /// </summary>
        FileInfo FileInfo { get; }

        /// <summary>
        /// Is tthere a currently opened and active document being edited?
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// The formatting options for this document.
        /// </summary>
        OptionSet FormattingOptions { get; }

        /// <summary>
        /// The current offset of the caret in this document.
        /// </summary>
        int CaretOffset { get; }

        /// <summary>
        /// Gets the <see cref="InteractionLocation"/> for the <see cref="IActiveDocument"/>.
        /// </summary>
        /// <param name="interactionOffset"></param>
        /// <returns></returns>
        InteractionLocation GetInteractionLocation(int? interactionOffset = null);

        /// <summary>
        /// The <see cref="Microsoft.CodeAnalysis.Document"/> for the currently active document.
        /// <para/>
        /// May be null for IDEs that do not yet fully support 
        /// </summary>
        Microsoft.CodeAnalysis.Document AnalysisDocument { get;  }

        /// <summary>
        /// Sets the caret location to the provided <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset"></param>
        void SetCaretOffset(int offset);

        void SetSelection(TextSpan textSpan);

        FileLocation OffsetToLocation(int start);

        string GetTextBetween(int startLine, int startColumn, int endLine, int endColumn);

        int LocationToOffset(int line, int column);

        /// <summary>
        /// Gets the object representing the Project File of the Active Document.
        /// </summary>
        IProjectFile ProjectFile { get; }
    }
}
