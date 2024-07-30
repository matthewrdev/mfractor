using System.Collections.Generic;
using MFractor.Workspace;

namespace MFractor.VS.Windows.WorkspaceModel
{
    /// <summary>
    /// An in-memory shallow copy of the DTE's loaded solution structure.
    /// <para/>
    /// This allows services to access the solution structure safely from background threads.
    /// </summary>
    public interface IWorkspaceShadowModel
    {
        /// <summary>
        /// The currently loaded solutions.
        /// </summary>
        IReadOnlyList<IdeSolution> Solutions { get; }

        /// <summary>
        /// Is a solution named <paramref name="solutionName"/> open?
        /// </summary>
        /// <param name="solutionName"></param>
        /// <returns></returns>
        bool HasSolution(string solutionName);

        /// <summary>
        /// Gets the 
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        IdeProject GetProjectByName(string projectName);

        IdeProject GetProjectByGuid(string projectGuid);

        IProjectFile GetProjectFile(string filePath);
    }
}
