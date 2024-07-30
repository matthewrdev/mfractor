using System;
using System.Collections.Generic;
using MFractor.CodeSnippets;

namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// Inserts a code snippet into the current document using the IDEs code snippet engine.
    /// </summary>
    public class InsertCodeSnippetWorkUnit : WorkUnit
    {
        /// <summary>
        /// The code snippet to insert.
        /// </summary>
        /// <value>The code snippet.</value>
        public ICodeSnippet CodeSnippet { get; set; }

        /// <summary>
        /// Gets or sets the file path to insert the new code snippet into.
        /// <para/>
        /// The file path can be either absolute or a relative/virtual path in a project.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; set; }

        /// <summary>
        /// The title of the dialog displayed to insert the code snippet.
        /// </summary>
        public string Title { get; set; } = "Insert Code Snippet";

        /// <summary>
        /// An optional URl to the help document for this feature.
        /// </summary>
        public string HelpUrl { get; set; } = string.Empty;

        /// <summary>
        /// The text to display in the confirm button.
        /// </summary>
        public string ConfirmButton { get; set; } = "Insert Code Snippet";

        /// <summary>
        /// Where the <see cref="CodeSnippet"/>'s contents should be insert.
        /// <para/>
        /// Leave as null to use the current caret location.
        /// </summary>
        public int InsertionOffset { get; set; }

        /// <summary>
        /// Fired when the value of a code snippet argument is edited.
        /// <para/>
        /// This allows editing of the code snipppet in workUnit to name changes. Return true to notify the caller the code snippet changed.
        /// </summary>
        public Func<string, ICodeSnippet, bool> OnArgumentValueEditedFunc { get; set; }

        public Func<ICodeSnippet, IReadOnlyList<IWorkUnit>> ApplyCodeSnippetFunc { get; set; }
    }
}
