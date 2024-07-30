using System;
using MFractor.Commands;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.Commands
{
    public interface IDocumentCommandContext : ICommandContext
    {
        string FilePath { get; }

        Project CompilationProject { get; }

        int CaretOffset { get; }

        InteractionLocation InteractionLocation { get; }
    }
}
