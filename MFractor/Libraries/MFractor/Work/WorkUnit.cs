using System;
using System.Collections.Generic;

namespace MFractor.Work
{
    /// <summary>
    /// The base class for an <see cref="IWorkUnit"/>, a unit of work that can be actioned by the platform or product MFractor is integrated into.
    /// </summary>
    public abstract class WorkUnit : IWorkUnit
    {
        /// <summary>
        /// Should this <see cref="IWorkUnit"/> be processed after all other work units?
        /// </summary>
        public bool IsPostProcessed { get; set; } = false;

        /// <summary>
        /// An identifier that uniquely identifies for this work unit.
        /// <para/>
        /// Defaults to a random guid, however, you can use this to "tag" a work unit for identification by other systems.
        /// <para/>
        /// For example, a code generator creates a series of work units to create some infrastructure, however, a UI that previews these work units needs to know about a specific <see cref="IWorkUnit"/>.
        /// <para/>
        /// Applying an <see cref="Identifier"/> let's this UI component identify that particular <see cref="IWorkUnit"/>.
        /// </summary>
        public virtual string Identifier { get; set; } = Guid.NewGuid().ToString();
    }
}
