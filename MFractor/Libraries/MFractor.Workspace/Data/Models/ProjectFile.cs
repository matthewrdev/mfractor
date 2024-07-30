using System;
using MFractor.Data.Models;

namespace MFractor.Workspace.Data.Models
{
    /// <summary>
    /// Represents a project file within a project.
    /// </summary>
    public class ProjectFile : GCEntity
    {
        /// <summary>
        /// The full file path of this file.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; set; }

        /// <summary>
        /// The name of this file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }
    }
}
