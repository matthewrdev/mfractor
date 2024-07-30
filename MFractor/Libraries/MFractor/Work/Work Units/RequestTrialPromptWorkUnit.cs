using System;
using MFractor.Analytics;

namespace MFractor.Work.WorkUnits
{
    public enum RequestTrialPromptMode
    {
        ToolbarOnly,

        PurchasePrompt,

        LaunchTrialDialog,
    }

    /// <summary>
    /// A <see cref="IWorkUnit"/> that shows an MFractor purchase push in the IDE.
    /// </summary>
    public class RequestTrialPromptWorkUnit : WorkUnit
    {
        public string Message { get; }

        public string DetailedMessage { get; }

        public string FeatureName { get; }

        public RequestTrialPromptMode RequestTrialPromptMode { get; set; } = RequestTrialPromptMode.PurchasePrompt;

        public RequestTrialPromptWorkUnit()
        {
        }

        public RequestTrialPromptWorkUnit(string message,
                                          string featureName)
        {
            Message = message;
            FeatureName = featureName;
        }

        public RequestTrialPromptWorkUnit(string message,
                                          IAnalyticsFeature analyticsFeature)
        {
            if (analyticsFeature is null)
            {
                throw new ArgumentNullException(nameof(analyticsFeature));
            }

            Message = message;
            FeatureName = analyticsFeature.AnalyticsEvent;
        }

        public RequestTrialPromptWorkUnit(string message,
                                          string detailedMessage,
                                          string featureName)
        {
            Message = message;
            DetailedMessage = detailedMessage;
            FeatureName = featureName;
        }

        public RequestTrialPromptWorkUnit(string message,
                                          string detailedMessage,
                                          IAnalyticsFeature analyticsFeature)
        {
            if (analyticsFeature is null)
            {
                throw new ArgumentNullException(nameof(analyticsFeature));
            }

            Message = message;
            DetailedMessage = detailedMessage;
            FeatureName = analyticsFeature.AnalyticsEvent;
        }
    }
}