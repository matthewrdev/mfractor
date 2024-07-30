using System;
using MFractor.Code.Documents;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.Documents
{
	public class ParsedCSharpDocument : ParsedDocument<SyntaxTree>, IParsedCSharpDocument
	{
        public ParsedCSharpDocument(string filePath, SyntaxTree syntaxTree, IProjectFile projectFile)
            : base(filePath, syntaxTree, projectFile)
		{
		}
	}
}
