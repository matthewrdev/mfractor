using System;
using System.ComponentModel.Composition;

namespace MFractor.Data.GarbageCollection
{
    /// <summary>
    /// An <see cref="IGarbageCollectionEvents"/> interface should be implemented to receive notifications for garbage collection cycles against a project resources database.
    /// <para/>
    /// To add a new garbage collection events handler to the <see cref="IResourcesDatabaseEngine"/>, apply the <see cref="System.Composition.Export{T}"/> attribute.
    /// </summary>
    [InheritedExport]
    public interface IGarbageCollectionEvents
    {
        /// <summary>
        /// Occurs when the database engine is about to start a garbage collection cycle on the provided database.
        /// <para/>
        /// Use this callback to prepare for garbage collection. For example, if an objects children are all marked for collection, mark it also for collection.
        /// </summary>
        /// <param name="database">Database.</param>
        void OnGarbageCollectionStarted(IDatabase database);

        /// <summary>
        /// Occurs after the provided database has been garbage collecteed.
        /// <para/>
        /// Use this callback to do any final cleanup after entities have been deleted.
        /// </summary>
        /// <param name="database">Database.</param>
        void OnGarbageCollectionEnded(IDatabase database);
    }
}
