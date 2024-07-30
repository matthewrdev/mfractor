namespace MFractor.Workspace.Data
{
    /// <summary>
    /// The event arguments for when a solution synchronisation state changes.
    /// </summary>
    public class SolutionSynchronisationStatusEventArgs
    {
        /// <summary>
        /// The name of the solution being synchronised by the <see cref="IResourcesDatabaseEngine"/>.
        /// </summary>
        /// <value>The name of the solution.</value>
        public string SolutionName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionSynchronisationStatusEventArgs"/> class.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        public SolutionSynchronisationStatusEventArgs(string solutionName)
        {
            SolutionName = solutionName;
        }
    }
}
