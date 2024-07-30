using System;
using MFractor.Code.Documents;

namespace MFractor.Code
{
    /// <summary>
    /// A filter to quickly discard groups of features from a given document, context and/or location.
    /// <para/>
    /// Execution filters should be static and single instance. as they are used as keys inside a features related engine.
    /// </summary>
    public sealed class DocumentExecutionFilter
    {
        /// <summary>
        /// Does this execution filter support the provided document?
        /// </summary>
        /// <value>The accepts document.</value>
        public Func<IParsedDocument, bool> AcceptsDocument { get; }

        /// <summary>
        /// Does the execution filter support the provided syntax?
        /// </summary>
        /// <value>The accepts syntax.</value>
        public Func<object, bool> AcceptsSyntax { get; }

        /// <summary>
        /// A human readable name of this execution filter for debugging purposes.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentExecutionFilter"/> class.
        /// </summary>
        /// <param name="name"> A human readable name of this execution filter for debugging purposes..</param>
        /// <param name="acceptsDocument">Does this execution filter support the provided document?</param>
        /// <param name="acceptsSyntaxFunc">Does the execution filter support the provided syntax?</param>
        public DocumentExecutionFilter(string name,
                                       Func<IParsedDocument, bool> acceptsDocument,
                                       Func<object, bool> acceptsSyntaxFunc)
        {
            Name = name;
            AcceptsDocument = acceptsDocument;
            AcceptsSyntax = acceptsSyntaxFunc;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentExecutionFilter"/> class.
        /// </summary>
        /// <param name="name"> A human readable name of this execution filter for debugging purposes.</param>
        /// <param name="acceptsDocument">Does this execution filter support the provided document?</param>
        public DocumentExecutionFilter(string name,
                              Func<IParsedDocument, bool> acceptsDocument)
            : this(name, acceptsDocument, null)
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="DocumentExecutionFilter"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="DocumentExecutionFilter"/>.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
