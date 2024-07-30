using System;
using MFractor.Data.GarbageCollection;

namespace MFractor.Data.Models
{
    /// <summary>
    /// A database entity that is automatically garbage collected by the <see cref="IDatabaseGarbageCollector"/> when <see cref="GCMarked"/> is set to true.
    /// </summary>
    public abstract class GCEntity : Entity
    {
        /// <summary>
        /// Is this entity marked for garbage collection?
        /// </summary>
        /// <value><c>true</c> if marked for garbage collection; otherwise, <c>false</c>.</value>
        public bool GCMarked { get; set; } = false;
    }
}
