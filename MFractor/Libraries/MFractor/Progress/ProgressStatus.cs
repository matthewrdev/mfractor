using System;

namespace MFractor.Progress
{
    /// <summary>
    /// The progress status of an operation.
    /// </summary>
    public class ProgressStatus
    {
        /// <summary>
        /// A description of the 
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; }

        public int WorkDone { get; }

        public int TotalWork { get; }

        public double Fraction => (double)WorkDone / (double)TotalWork;

        public ProgressStatus(string description,
                              int workDone,
                              int totalWork)
        {
            Description = description;
            WorkDone = workDone;
            TotalWork = totalWork;
        }
    }
}
