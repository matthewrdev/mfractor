using System;
using System.Collections.Generic;

namespace MFractor.Analytics
{
    /// <summary>
    /// The analytics service is used to submit user events to a remote analytics sink.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Track the specified feature.
        /// </summary>
        /// <returns>The track.</returns>
        /// <param name="feature">Feature.</param>
        void Track(IAnalyticsFeature feature);

        /// <summary>
        /// Track the specified feature and traits.
        /// </summary>
        /// <returns>The track.</returns>
        /// <param name="feature">Feature.</param>
        /// <param name="traits">Traits.</param>
        void Track(IAnalyticsFeature feature, IReadOnlyDictionary<string, string> traits);

        /// <summary>
        /// Track the specified eventName.
        /// <para/>
        /// Null or empty <paramref name="eventName"/> values will be ignore.
        /// </summary>
        /// <returns>The track.</returns>
        /// <param name="eventName">Event name.</param>
		void Track(string eventName);

        /// <summary>
        /// Track the specified eventName and traits.
        /// <para/>
        /// Null or empty <paramref name="eventName"/> values will be ignore.
        /// </summary>
        /// <returns>The track.</returns>
        /// <param name="eventName">Event name.</param>
        /// <param name="traits">Traits.</param>
		void Track(string eventName, IReadOnlyDictionary<string, string> traits);

        /// <summary>
        /// Tracks the specified <paramref name="ex"/>.
        /// </summary>
        /// <param name="ex">Ex.</param>
        void Track(Exception ex);

        /// <summary>
        /// Tracks the specified <paramref name="@enum"/>.
        /// </summary>
        /// <param name="ex">Ex.</param>
        void Track(Enum @enum);
    }
}

