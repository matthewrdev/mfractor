using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MFractor.Work.WorkUnits
{
    // TODO: Will need a delegate?

    /// <summary>
    /// An <see cref="IWorkUnit"/> that opens the import icon wizard dialog in the IDE.
    /// <para/>
    /// This allows users to easily import application icons for their Android and iOS projects.
    /// </summary>
    public class ImportIconWizardWorkUnit : WorkUnit
    {
        // TODO: Implement
        public IReadOnlyList<Project> Projects { get; set; } = new List<Project>();
    }
}
