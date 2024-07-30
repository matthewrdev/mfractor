using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Ide.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that opens the <see cref="FilePath"/> and selects the <see cref="Span"/> in the text editor.
    /// </summary>
    public class NavigateToFileSpanWorkUnit : WorkUnit
    {
        public TextSpan Span { get; }

        public string FilePath { get; }

        public Project Project { get; }

        public bool HighlightSpan { get; }

        public NavigateToFileSpanWorkUnit(TextSpan span, 
                                          string filePath, 
                                          Project project = null, 
                                          bool highlightSpan = true)
        {
            Span = span;
            FilePath = filePath;
            Project = project;
            HighlightSpan = highlightSpan;
        }
    }
}
