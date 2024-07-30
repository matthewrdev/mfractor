using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor;
using MFractor.Work.WorkUnits;
using MFractor.Licensing;
using MFractor.Commands.Attributes;

namespace MFractor.iOS.Commands
{
    [RequiresLicense]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class StartAccessibilityInspectorCommand : ICommand, IAnalyticsFeature
    {
        const string accessiblityInspectorAppPath = "/Applications/Xcode.app/Contents/Applications/Accessibility Inspector.app/Contents/MacOS/Accessibility Inspector";

        public string AnalyticsEvent => "Start Accessibility Inspector";

        public void Execute(ICommandContext commandContext)
        {
            Process.Start(accessiblityInspectorAppPath);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            if (File.Exists(accessiblityInspectorAppPath))
            {
                return new CommandState(true, true, "Accessibility Inspector", "Launch the Accessibility Inspector");
            }

            return default;
        }
    }
}
