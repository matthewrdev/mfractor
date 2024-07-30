using System;
using Microsoft.CodeAnalysis;

namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that deletes the given <see cref="FilePath"/> in the <s .
    /// </summary>
    public class DeleteFileWorkUnit : WorkUnit
    {
        /// <summary>
        /// The full path to the file to delete.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Optional: The project that this file belongs to.
        /// <para/>
        /// If null, MFractor will simply delete the <see cref="FilePath"/>
        /// </summary>
        public Project Project { get; set; }
    }
}
