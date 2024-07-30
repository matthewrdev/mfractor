using System.Collections.Generic;
using System.IO;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Workspace.Data.Indexes;
using MFractor.Workspace.Data.Models;

namespace MFractor.Workspace.Data.Repositories
{
    /// <summary>
    /// The <see cref="ProjectFileRepository"/> provides data access methods to the <see cref="ProjectFile"/> table in the project resources database.
    /// </summary>
    public class ProjectFileRepository : EntityRepository<ProjectFile>
    {
        public ProjectFileRepository()
            : base(new ProjectFileByFilePathIndex())
        {
        }
        
        /// <summary>
        /// Gets the project file by identifier.
        /// </summary>
        /// <returns>The project file by identifier.</returns>
        /// <param name="id">Identifier.</param>
        public ProjectFile GetProjectFileFor(ProjectFileOwnedEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return GetProjectFileById(entity.ProjectFileKey);
        }

        /// <summary>
        /// Gets the project file by identifier.
        /// </summary>
        /// <returns>The project file by identifier.</returns>
        /// <param name="id">Identifier.</param>
        public ProjectFile GetProjectFileById(int id)
        {
            var projectFile = this.Get(id);

            if (projectFile == null || projectFile.GCMarked)
            {
                return null;
            }

            return projectFile;
        }

        /// <summary>
        /// Gets the project file by file path.
        /// </summary>
        /// <returns>The project file by file path.</returns>
        /// <param name="filePath">File path.</param>
        public ProjectFile GetProjectFileByFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var index = GetEntityIndex<ProjectFileByFilePathIndex>();

            return index.GetByFilePath(filePath).FirstOrDefault(pf => !pf.GCMarked);
        }

        /// <summary>
        /// Creates a new project file in the database using the provided <paramref name="filePath"/>.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="filePath">File path.</param>
        public ProjectFile CreateFile(string filePath)
		{
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var file = new ProjectFile
            {
                PrimaryKey = NextPrimaryKey(),
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };
            Insert(file);

            return file;
		}
	}
}
