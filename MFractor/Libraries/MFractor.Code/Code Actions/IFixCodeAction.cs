using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// A code action that fixes issues reported by a code analysers.
    /// </summary>
    public interface IFixCodeAction : ICodeAction
    {
        /// <summary>
        /// The <see cref="IXmlSyntaxCodeAnalyser"/> that this code fix targets.
        /// </summary>
        /// <value>The type of the code analyser.</value>
        Type TargetCodeAnalyser { get; }

        IEnumerable<Type> TargetCodeAnalysers { get; }
    }
}
