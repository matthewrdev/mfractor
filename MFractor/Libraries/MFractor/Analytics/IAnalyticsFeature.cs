using System;

namespace MFractor.Analytics
{
    /// <summary>
    /// A feature that should be tracked via analytics using it's <see cref="IAnalyticsFeature.AnalyticsEvent"/>.
    /// <para/>
    /// To track a feature, use the Track methods on <see cref="IAnalyticsService"/>.
    /// </summary>
    public interface IAnalyticsFeature
    {
        /// <summary>
        /// The name of the analytics feature that should be tracked.
        /// </summary>
        /// <value>The analytics event.</value>
        string AnalyticsEvent { get; }
    }
}
