using System.Collections.Generic;
using MFractor.Progress;

namespace MFractor.Text
{
    /// <summary>
    /// Applies a series of <see cref="ITextReplacement"/>'s into the IDEs text documents (either using the corresponding open document or file on disk).
    /// <para/>
    /// When applying a series of <see cref="ITextReplacement"/>'s, the service sort them into descending order and then apply them.
    /// <para/>
    /// This generally means that you do not need to sort the replacements or worry about them conflicting with each other.
    /// </summary>
    public interface ITextReplacementService
    {
        void ApplyTextReplacements(IReadOnlyDictionary<string, IReadOnlyList<ITextReplacement>> replacements, IProgressMonitor progressMonitor);

        void ApplyTextReplacements(IReadOnlyList<ITextReplacement> replacements, IProgressMonitor progressMonitor);

        void ApplyTextReplacements(string fileName, IReadOnlyList<ITextReplacement> replacements, IProgressMonitor progressMonitor);
    }
}
