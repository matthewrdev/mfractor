using System;
using System.Collections.Generic;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Data.Schemas;

namespace MFractor.Workspace.Data
{
    /// <summary>
    /// A database containing the resources meta-data information for a project.
    /// </summary>
    class ProjectResourcesDatabase : Database, IProjectResourcesDatabase
    {
        /// <summary>
        /// Is this project resources database valid?
        /// <para/>
        /// If <see cref="IsValid"/> is false, do not use the data returned by the database as a synchronisation pass is likely in progress.
        /// </summary>
        /// <value><c>true</c> if is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get; internal set; } = true;

        /// <summary>
        /// The name of the solution that this database belongs to.
        /// </summary>
        /// <value>The name of the solution.</value>
        public string SolutionName { get; }

        /// <summary>
        /// The GUID of the project that this resources database maps to.
        /// </summary>
        /// <value>The project GUID.</value>
        public string ProjectGuid { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectResourcesDatabase"/> class.
        /// </summary>
        public ProjectResourcesDatabase(string solutionName,
                                        string projectGuid,
                                        IEnumerable<ISchema> schemas,
                                        IEnumerable<IRepositoryCollection> repositoryCollections)
            : base(schemas, repositoryCollections)
        {
            SolutionName = solutionName;
            ProjectGuid = projectGuid;
        }

        public void Insert(Entity entity)
        {
            if (entity is null)
            {
                return;
            }

            var repo = GetRepositoryForEntityType(entity.GetType());
            if (repo != null)
            {
                repo.InsertEntity(entity);
            }
        }

        public void Update(Entity entity)
        {
            if (entity is null)
            {
                return;
            }

            var repo = GetRepositoryForEntityType(entity.GetType());
            if (repo != null)
            {
                repo.UpdateEntity(entity);
            }
        }
    }
}
