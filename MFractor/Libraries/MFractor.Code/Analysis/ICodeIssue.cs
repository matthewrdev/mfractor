using System;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Code.Analysis
{
    /// <summary>
    /// An issue for a segment of code.
    /// </summary>
    public interface ICodeIssue : IIssue
    {
        /// <summary>
        /// The text span of the code issue.
        /// </summary>
        /// <value>The span.</value>
        TextSpan Span { get; }

        /// <summary>
        /// Gets the category of this code issue.
        /// </summary>
        /// <value>The category.</value>
        IssueClassification Classification { get; }

        /// <summary>
        /// Is this code issue a silent warning, aka, should it trigger the tooltip to display and a code annotation to apply?
        /// <para/>
        /// Silent tooltips detect issues that then activate fixes/refactorings without appearing in the user interface.
        /// </summary>
        bool IsSilent { get; }

        /// <summary>
        /// Get's any additional content attached to this issue, cast as <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The additional content.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T GetAdditionalContent<T>();
    }
}
