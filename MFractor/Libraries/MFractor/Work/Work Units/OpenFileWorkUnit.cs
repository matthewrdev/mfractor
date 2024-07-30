using System;
using Microsoft.CodeAnalysis;

namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that opens the given <see cref="FilePath"/> for the <see cref="Project"/> in the IDE.
    /// </summary>
    public class OpenFileWorkUnit : WorkUnit
    {
        public string FilePath { get; }

        public Project Project { get; }

        /// <summary>
        /// An action to invoke after the <see cref="FilePath"/> is opened within the IDE.
        /// </summary>
        public Action OnDocumentOpenedAction { get; }

        public OpenFileWorkUnit(string filePath, 
                                Project project,
                                Action onDocumentOpenedAction = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            FilePath = filePath;
            Project = project;
            OnDocumentOpenedAction = onDocumentOpenedAction ?? new Action(() => { });
        }
    }
}
