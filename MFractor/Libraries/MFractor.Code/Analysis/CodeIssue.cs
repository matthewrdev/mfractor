using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Code.Analysis
{
    /// <summary>
    /// The result of an ICodeAnalyser; an issue within the users source code.
    /// </summary>
    [DebuggerDisplay("{Category} - {Message} - {DiagnosticId}")]
    class CodeIssue : ICodeIssue
    {
        const string additionalContentKey = "com.mfractor.code.analysis.additional_content";

        /// <summary>
        /// A concise message that describes the code issue to the user.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; }

        /// <summary>
        /// The code analyser that found this issue. 
        /// 
        /// This will be used to locate any fix-generators that are associated with this code analyser.
        /// </summary>
        /// <value>The analyser identifier.</value>
        public Type AnalyserType { get; }

        /// <summary>
        /// The diagnostic id that this code issue targets.
        /// </summary>
        public string DiagnosticId { get; }

        /// <summary>
        /// The MFractor identifier of that this code issue targets.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// The region of the code issue within the document.
        /// </summary>
        /// <value>The region.</value>
        public TextSpan Span { get; }

        /// <summary>
        /// The issue classification
        /// </summary>
        public IssueClassification Classification { get; }

        /// <summary>
        /// Is this code issue a silent warning, aka, should it trigger the tooltip to display and a code annotation to apply?
        /// <para/>
        /// Silent tooltips detect issues that then activate fixes/refactorings without appearing in the user interface.
        /// </summary>
        public bool IsSilent { get; }

        /// <summary>
        /// The syntax that this code issue is targetted against.
        /// </summary>
        public object Syntax { get; }

        object additionalContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.CodeAnalysis.CodeIssue"/> class.
        /// </summary>
        /// <param name="analyserType">Analyser type.</param>
        /// <param name="message">Message.</param>
        /// <param name="span">Span.</param>
        /// <param name="issueType">Issue type.</param>
        /// <param name="metaData">Meta data.</param>
        public CodeIssue(Type analyserType,
                         string diagnosticId,
                         string identifier,
                         string message,
                         TextSpan span,
                         IssueClassification issueType,
                         bool isSilent,
                         object syntax)
        {
            if (string.IsNullOrEmpty(diagnosticId))
            {
                throw new ArgumentException("message", nameof(diagnosticId));
            }

            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("message", nameof(identifier));
            }

            AnalyserType = analyserType ?? throw new ArgumentNullException(nameof(analyserType));
            DiagnosticId = diagnosticId;
            Identifier = identifier;
            Message = message;
            Span = span;
            Classification = issueType;
            IsSilent = isSilent;
            Syntax = syntax;
        }

        /// <summary>
        /// Get's any additional content attached to this issue, cast as <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The additional content.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetAdditionalContent<T>()
        {
            try
            {
                return (T)additionalContent;
            }
            catch { }

            return default;
        }

        /// <summary>
        /// Attaches any additional content to this issue.
        /// </summary>
        /// <param name="value">Value.</param>
        public void SetAdditionalContent(object value)
        {
            additionalContent = value;
        }
    }

}
