using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that launches the app icon importer targeting the provided <see cref="Projects"/>.
    /// </summary>
    public class AppIconImporterWorkUnit : WorkUnit
    {
        /// <summary>
        /// The projects to target in the app icon wizard.
        /// </summary>
        /// <value>The projects.</value>
        IReadOnlyList<Project> Projects { get; set; }
    }
}
