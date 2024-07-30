using System;
using MFractor.CodeSnippets;

namespace MFractor.Views.CodeSnippets
{
    public class InsertCodeSnippetEventArgs : EventArgs
    {
        public InsertCodeSnippetEventArgs(ICodeSnippet codeSnippet)
        {
            CodeSnippet = codeSnippet;
        }

        public ICodeSnippet CodeSnippet { get; }
    }
}
