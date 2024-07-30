using System;
using System.Collections;
using System.Collections.Generic;
using MFractor.Data;
using MFractor.Data.Models;

namespace MFractor.Data.GarbageCollection
{
    /// <summary>
    /// When 
    /// </summary>
    public enum MarkOperation
    {
        /// <summary>
        /// Mark only the provided entity for garbage collection.
        /// </summary>
        FileOnly,

        /// <summary>
        /// Mark only children of the provided entity for garbage collection.
        /// </summary>
        ChildrenOnly,

        /// <summary>
        /// Mark both the provided entity and all its children for garbage collection.
        /// </summary>
        FileAndChildren,
    }

	public interface IDatabaseGarbageCollector
	{
        /// <summary>
        /// Garbage collects the provided database, finding all GCEntity's that have GCMarked set to <see langword="true"/> and deleting them.
        /// </summary>
        /// <param name="database">Database.</param>
		void GarbageCollect (IDatabase database);
	}
}
