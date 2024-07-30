using System;
using System.Collections.Generic;
using MFractor.Data;
using MFractor.Workspace.Data.Models;

namespace MFractor.Workspace.Data
{
    /// <summary>
    /// A database containing the resources meta-data information for a project.
    /// </summary>
    public interface IProjectResourcesDatabase : IDatabase
    {
        /// <summary>
        /// Is this project resources database valid?
        /// <para/>
        /// If <see cref="IsValid"/> is false, do not use the data returned by the database as a synchronisation pass is likely in progress.
        /// </summary>
        /// <value><c>true</c> if is valid; otherwise, <c>false</c>.</value>
        bool IsValid { get; }

        /// <summary>
        /// The name of the solution that this database belongs to.
        /// </summary>
        /// <value>The name of the solution.</value>
        string SolutionName { get; }

        /// <summary>
        /// The GUID of the project that this resources database maps to.
        /// </summary>
        /// <value>The project GUID.</value>
        string ProjectGuid { get; }

        /// <summary>
        /// Inserts the entity into the datastores.
        /// </summary>
        /// <param name="entity"></param>
        void Insert(Entity entity);

        /// <summary>
        /// Updates the entity into the 
        /// </summary>
        /// <param name="entity"></param>
        void Update(Entity entity);
    }
}