using System;
using MFractor.Ide.DeleteOutputFolders;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that triggers a delete output folders operation on the provided <see cref="Solution"/> or <see cref="Project"/>
    /// <para/>
    /// If no <see cref="Options"/> are provided, MFractor will try to load the users preferences.
    /// </summary>
    public class DeleteOutputFoldersWorkUnit : WorkUnit
    {
        /// <summary>
        /// The solution to delete the output folders from.
        /// </summary>
        public Solution Solution { get; set; }

        /// <summary>
        /// The project to delete the output folders from.
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// The options to use when deleting the output folders.
        /// </summary>
        public IDeleteOutputFoldersOptions Options { get; set; }
    }
}
