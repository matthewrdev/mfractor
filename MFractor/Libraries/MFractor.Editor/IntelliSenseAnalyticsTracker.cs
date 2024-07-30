using System;
using System.Collections.Generic;
using MFractor.Analytics;

namespace MFractor.Editor
{
    class IntelliSenseAnalyticsTracker : IDisposable
    {
        public IntelliSenseAnalyticsTracker(IAnalyticsService analyticsService,
                                            IAnalyticsFeature analyticsFeature)
        {
            this.analyticsService = analyticsService;
            this.analyticsFeature = analyticsFeature;
        }

        readonly DateTime sessionStart = DateTime.UtcNow;
        readonly TimeSpan triggerAnalyticsTimeSpan = new TimeSpan(0, 0, 20);
        readonly IAnalyticsService analyticsService;
        DateTime? lastUsage;
        TimeSpan? sessionUsageSpan;

        readonly IAnalyticsFeature analyticsFeature;

        public void TrackIntelliSenseSession()
        {
            var now = System.DateTime.UtcNow;

            if (lastUsage != null)
            {
                if (sessionUsageSpan == null)
                {
                    sessionUsageSpan = now - lastUsage.Value;
                }
                else
                {
                    sessionUsageSpan = sessionUsageSpan.Value + (now - lastUsage.Value);
                }

                if (sessionUsageSpan != null && sessionUsageSpan.Value >= triggerAnalyticsTimeSpan)
                {
                    ReportSession();
                    sessionUsageSpan = null;
                }
            }

            lastUsage = System.DateTime.UtcNow;
        }

        void ReportSession()
        {
            var duration = (System.DateTime.UtcNow - sessionStart);

            var parts = new List<string>();
            if (duration.TotalHours > 0)
            {
                parts.Add(((int)Math.Floor(duration.TotalHours)).ToString() + " hours");
            }
            if (duration.Minutes > 0)
            {
                parts.Add(duration.Minutes.ToString() + " minutes");
            }
            if (duration.TotalHours > 0)
            {
                parts.Add(duration.Seconds + " seconds");
            }

            var sessionLength = string.Join(", ", parts);

            analyticsService.Track(analyticsFeature, new Dictionary<string, string>() { { "SessionLength", sessionLength } });
        }

        public void Dispose()
        {

        }
    }
}
