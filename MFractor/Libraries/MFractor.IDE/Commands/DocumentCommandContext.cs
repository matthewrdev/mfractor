using Microsoft.CodeAnalysis;

namespace MFractor.Ide.Commands
{
    public class DocumentCommandContext : IDocumentCommandContext
    {
        public DocumentCommandContext(string filePath,
                                      Project compilationProject,
                                      int caretOffset,
                                      InteractionLocation interactionLocation)
        {
            FilePath = filePath;
            CompilationProject = compilationProject;
            CaretOffset = caretOffset;
            InteractionLocation = interactionLocation ?? new InteractionLocation(caretOffset);
        }

        public string FilePath { get; }

        public Project CompilationProject { get; }

        public int CaretOffset { get; }

        public InteractionLocation InteractionLocation { get; }
    }
}