using System;

namespace MFractor.Workspace.Data
{
    /// <summary>
    /// The event arguments for when a project synchronisation pass completes changes.
    /// </summary>
    public class ProjectSynchronisationPassCompletedEventArgs : EventArgs
    {        public ProjectSynchronisationPassCompletedEventArgs(string projectGuid)
        {
            ProjectGuid = projectGuid;
        }

        /// <summary>
        /// The guid of the project that was synchronised.
        /// </summary>
        public string ProjectGuid { get; }
    }
}
