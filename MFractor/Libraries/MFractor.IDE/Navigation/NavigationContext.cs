using System;
using Microsoft.CodeAnalysis;

namespace MFractor.Navigation
{
    public class NavigationContext : INavigationContext
    {
        public NavigationContext(string filePath, Project compilationProject, int caretOffset, InteractionLocation interactionLocation)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            FilePath = filePath;
            CompilationProject = compilationProject ?? throw new ArgumentNullException(nameof(compilationProject));
            CaretOffset = caretOffset;
            InteractionLocation = interactionLocation ?? throw new ArgumentNullException(nameof(interactionLocation));
        }

        public string FilePath { get; }

        public Project CompilationProject { get; }

        public int CaretOffset { get; }

        public InteractionLocation InteractionLocation { get; }
    }
}