using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class PreferencesCommand : WorkUnitCommand, IAnalyticsFeature
    {
        [ImportingConstructor]
        public PreferencesCommand(Lazy<IWorkEngine> workEngine)
            : base("Preferences", "Open MFractors preferences.", new OpenPreferencesWorkUnit(), workEngine)
        {
        }

        public string AnalyticsEvent => "Open MFractor Preferences";
    }
}
