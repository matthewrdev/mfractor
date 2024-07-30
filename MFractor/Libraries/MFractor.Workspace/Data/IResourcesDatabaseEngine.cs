using System;
using MFractor.Workspace.Data.Synchronisation;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace.Data
{
    /// <summary>
    /// The resources database engine maintains a <see cref="IProjectResourcesDatabase"/> for each project in all workspaces.
    /// <para/>
    /// The <see cref="IResourcesDatabaseEngine"/> listens to changes in solutions, projects and files converts file changes into a queryable SQLite database that describes a project.
    /// <para/>
    /// To opt-into the synchronisation process, implement the <see cref="ITextResourceSynchroniser"/> to synchronise text files or <see cref="IAssetResourceSynchroniser"/> to synchronise binary files.
    /// </summary>
    public interface IResourcesDatabaseEngine
    {
        /// <summary>
        /// Gets the <see cref="IProjectResourcesDatabase"/> for the provided project <paramref name="guid"/>.
        /// <para/>
        /// Before using this database, ensure to check that it is not <see cref="null"/> and that the <see cref="IProjectResourcesDatabase.IsValid"/> flag is <see cref="true"/>.
        /// </summary>
        IProjectResourcesDatabase GetProjectResourcesDatabase(string guid);

        /// <summary>
        /// Gets the <see cref="IProjectResourcesDatabase"/> for the provided <paramref name="project"/>.
        /// <para/>
        /// Before using this database, ensure to check that it is not <see cref="null"/> and that the <see cref="IProjectResourcesDatabase.IsValid"/> flag is <see cref="true"/>.
        /// </summary>
        IProjectResourcesDatabase GetProjectResourcesDatabase(Project project);

        /// <summary>
        /// Gets the <see cref="IProjectResourcesDatabase"/> for the provided <paramref name="projectIdentifier"/>.
        /// <para/>
        /// Before using this database, ensure to check that it is not <see cref="null"/> and that the <see cref="IProjectResourcesDatabase.IsValid"/> flag is <see cref="true"/>.
        /// </summary>
        IProjectResourcesDatabase GetProjectResourcesDatabase(ProjectIdentifier projectIdentifier);

        /// <summary>
        /// Synchonises the resources database for the given <paramref name="solution"/>.
        /// <para/>
        /// As this enqueues a background synchronisation to take place, subscribe to the <see cref="SolutionSyncStarted"/> and <see cref="SolutionSyncEnded"/> events to be notified when this operation finishes.
        /// </summary>
        void SynchroniseSolutionResources(Solution solution);

        /// <summary>
        /// Occurs when a solution sync has started.
        /// </summary>
        event EventHandler<SolutionSynchronisationStatusEventArgs> SolutionSyncStarted;

        /// <summary>
        /// Occurs when a solution sync has ended.
        /// </summary>
        event EventHandler<SolutionSynchronisationStatusEventArgs> SolutionSyncEnded;

        /// <summary>
        /// Occurs when the synchronisation pass for a project has been completed.
        /// <para/>
        /// Synchonisation passes are triggered in response to workspace events. See <see cref="IWorkspaceService"/> for the various event kinds.
        /// </summary>
        event EventHandler<ProjectSynchronisationPassCompletedEventArgs> ProjectSynchronisationPassCompleted;
    }
}
