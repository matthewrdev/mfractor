using System;
using MFractor.Data;
using MFractor.Data.GarbageCollection;
using MFractor.Workspace.Data.Models;

namespace MFractor.Workspace.Data.GarbageCollection
{
    public interface IProjectFileDatabaseGarbageCollectionService
    {
        /// <summary>
        /// Flags the provided <paramref name="file"/> for garbage collection, marking itself and all dependent project file owned entities for deletion.
        /// </summary>
        /// <param name="database">Database.</param>
        /// <param name="file">File.</param>
        /// <param name="operation">Operation.</param>
        void Mark(IDatabase database, ProjectFile file, MarkOperation operation);

        /// <summary>
        /// Unflags the provided <paramref name="file"/> for garbage collection, marking itself and all dependent project file owned entities for deletion.
        /// </summary>
        /// <param name="database">Database.</param>
        /// <param name="file">File.</param>
        /// <param name="operation">Operation.</param>
        void UnMark(IDatabase database, ProjectFile file, MarkOperation operation);
    }
}
