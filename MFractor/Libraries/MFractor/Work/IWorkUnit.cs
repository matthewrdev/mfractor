using System.Collections.Generic;

namespace MFractor.Work
{
    /// <summary>
    /// A unit of work that can be actioned by whatever platform or product MFractor is integrated into.
    /// <para/>
    /// To create a new workUnit, implement this interface and then create a <see cref="IWorkUnitHandler"/> that can apply the changes a workUnit represents into meaningful actions.
    /// </summary>
    public interface IWorkUnit
    {
        /// <summary>
        /// Should this <see cref="IWorkUnit"/> be processed after all other work units?
        /// </summary>
        bool IsPostProcessed { get; set; }

        /// <summary>
        /// An identifier that uniquely identifies for this work unit.
        /// <para/>
        /// Defaults to a random guid, however, you can use this to "tag" a work unit for identification by other systems.
        /// <para/>
        /// For example, a code generator creates a series of work units to create some infrastructure, however, a UI that previews these work units needs to know about a specific <see cref="IWorkUnit"/>.
        /// <para/>
        /// Applying an <see cref="Identifier"/> let's this UI component identify that particular <see cref="IWorkUnit"/>.
        /// </summary>
        string Identifier { get; }
    }
}
