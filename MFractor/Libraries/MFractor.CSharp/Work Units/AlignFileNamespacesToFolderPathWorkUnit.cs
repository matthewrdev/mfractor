using System;
using System.Collections.Generic;
using MFractor.Workspace;

namespace MFractor.CSharp.WorkUnits
{
    public class AlignFileNamespacesToFolderPathWorkUnit
    {
        /// <summary>
        /// The project files to correct the namespaces in
        /// </summary>
        public IReadOnlyList<IProjectFile> Files { get; set; }
    }
}
