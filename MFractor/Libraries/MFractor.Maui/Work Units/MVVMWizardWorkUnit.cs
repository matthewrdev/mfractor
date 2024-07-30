using System;
using System.Collections.Generic;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;

namespace MFractor.Maui.WorkUnits
{
    /// <summary>
    /// Launches the MVVM Wizard.
    /// </summary>
    public class MVVMWizardWorkUnit : WorkUnit
    {
        public ProjectIdentifier TargetProject { get; set; }

        public IXamlPlatform Platform { get; set; }

        public IReadOnlyList<ProjectIdentifier> Projects { get; set; }
    }
}
