using Microsoft.CodeAnalysis;

namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> the provided <see cref="FilePath"/> in the operating systems file explorer.
    /// </summary>
    public class OpenFileInExplorerWorkUnit : WorkUnit
    {
        public string FilePath { get; }

        public Project Project { get; }

        public OpenFileInExplorerWorkUnit(string filePath,
                                    Project project)
        {
            FilePath = filePath;
            Project = project;
        }
    }
}
