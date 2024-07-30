using System;
using System.Collections.Generic;

namespace MFractor.Code.Analysis
{
    public class CodeAnalysisStateChangedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<string, bool> CodeAnalysers { get; }

        public CodeAnalysisStateChangedEventArgs(IReadOnlyDictionary<string, bool> codeAnalysers)
        {
            CodeAnalysers = codeAnalysers;
        }

        public CodeAnalysisStateChangedEventArgs(string identifier, bool enabled)
            : this(new Dictionary<string, bool>()
            {
                { identifier, enabled }
            })
        {
        }
    }
}
