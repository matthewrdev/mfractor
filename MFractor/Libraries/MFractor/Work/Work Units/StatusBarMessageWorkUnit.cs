using System;
namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// Shows a message in the IDEs status bar.
    /// </summary>
    public class StatusBarMessageWorkUnit : WorkUnit
    {
        /// <summary>
        /// The message to show in the status bar.
        /// </summary>
        public string Message { get; set; }
    }
}
