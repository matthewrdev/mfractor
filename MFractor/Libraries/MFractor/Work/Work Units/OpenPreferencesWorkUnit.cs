using System;
namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> than opens MFractors Preferences panel.
    /// </summary>
    public class OpenPreferencesWorkUnit : WorkUnit
    {
        /// <summary>
        /// The ID of the preferences panel to open.
        /// <para/>
        /// When null or empty, defaults to opening MFractors default preferences panel.
        /// </summary>
        public string PreferencesId { get; set; }
    }
}
