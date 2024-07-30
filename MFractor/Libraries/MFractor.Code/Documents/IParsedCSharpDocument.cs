using System;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.Documents
{
    public interface IParsedCSharpDocument : IParsedDocument
    {
        SyntaxTree GetSyntaxTree();
    }
}
