using System;
using Microsoft.CodeAnalysis;

namespace MFractor.Navigation
{
    public interface INavigationContext
    {
        string FilePath { get; }

        Project CompilationProject { get; }

        int CaretOffset { get; }

        InteractionLocation InteractionLocation { get; }
    }
}