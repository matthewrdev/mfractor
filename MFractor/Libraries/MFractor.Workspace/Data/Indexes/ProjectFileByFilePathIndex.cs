using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Workspace.Data.Models;

namespace MFractor.Workspace.Data.Indexes
{
    class ProjectFileByFilePathIndex : IEntityIndex<ProjectFile>
    {
        class ProjectFileIndexEntry
        {
            public ProjectFileIndexEntry(string filePath)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentException($"'{nameof(filePath)}' cannot be null or empty.", nameof(filePath));
                }

                FilePath = filePath;
            }

            readonly Dictionary<int, ProjectFile> projectFiles = new Dictionary<int, ProjectFile>();

            public string FilePath { get; }

            public void Add(ProjectFile projectFile)
            {
                if (projectFile is null)
                {
                    throw new ArgumentNullException(nameof(projectFile));
                }

                projectFiles[projectFile.PrimaryKey] = projectFile;
            }

            public void Remove(ProjectFile projectFile)
            {
                if (projectFile is null)
                {
                    throw new ArgumentNullException(nameof(projectFile));
                }

                Remove(projectFile.PrimaryKey);
            }

            public void Remove(int primaryKey)
            {
                if (projectFiles.ContainsKey(primaryKey))
                {
                    projectFiles.Remove(primaryKey);
                }
            }

            public int Count => projectFiles.Count;

            public IReadOnlyList<ProjectFile> ProjectFiles => projectFiles.Values.ToList();
        }

        readonly object indexLock = new object();
        readonly Dictionary<string, ProjectFileIndexEntry> filePathIndex = new Dictionary<string, ProjectFileIndexEntry>();
        readonly Dictionary<int, ProjectFileIndexEntry> primaryKeyIndex = new Dictionary<int, ProjectFileIndexEntry>();

        public void Clear()
        {
            lock (indexLock)
            {
                filePathIndex.Clear();
                primaryKeyIndex.Clear();
            }
        }

        void Remove(int primaryKey)
        {
            if (primaryKeyIndex.TryGetValue(primaryKey, out var index))
            {
                primaryKeyIndex.Remove(primaryKey);
                index.Remove(primaryKey);

                if (index.Count == 0 && filePathIndex.ContainsKey(index.FilePath))
                {
                    filePathIndex.Remove(index.FilePath);
                }
            }
        }

        public void OnDeleted(int entityPrimaryKey)
        {
            lock (indexLock)
            {
                Remove(entityPrimaryKey);
            }
        }

        public void OnDeleted(IReadOnlyList<int> primaryKeys)
        {
            lock (indexLock)
            {
                foreach (var key in primaryKeys)
                {
                    Remove(key);
                }
            }
        }

        public void OnInserted(ProjectFile entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            lock (indexLock)
            {
                var filePath = entity.FilePath;

                ProjectFileIndexEntry entry = null;
                if (!filePathIndex.TryGetValue(filePath, out entry))
                {
                    entry = new ProjectFileIndexEntry(filePath);
                    filePathIndex[filePath] = entry;
                }

                primaryKeyIndex[entity.PrimaryKey] = entry;
                entry.Add(entity);
            }
        }

        public void OnUpdated(ProjectFile before, ProjectFile after)
        {
            if (before.FilePath != after.FilePath)
            {
                Remove(before.PrimaryKey);
                OnInserted(after);
            }
        }

        public IReadOnlyList<ProjectFile> GetByFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new List<ProjectFile>();
            }

            if (!filePathIndex.ContainsKey(filePath))
            {
                return new List<ProjectFile>();
            }

            return filePathIndex[filePath].ProjectFiles;
        }
    }
}
