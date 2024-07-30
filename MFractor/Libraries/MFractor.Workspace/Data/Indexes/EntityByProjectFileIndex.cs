using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Workspace.Data.Models;

namespace MFractor.Workspace.Data.Indexes
{
    public class EntityByProjectFileIndex<TEntity> : IEntityIndex<TEntity> where TEntity : ProjectFileOwnedEntity
    {
        class Index
        {
            public Index(int projectFilePrimaryKey)
            {
                ProjectFilePrimaryKey = projectFilePrimaryKey;
            }

            readonly Dictionary<int, TEntity> entities = new Dictionary<int, TEntity>();

            public void Add(TEntity entity)
            {
                if (entity is null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                entities[entity.PrimaryKey] = entity;
            }

            public void Remove(TEntity entity)
            {
                if (entity is null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                Remove(entity.PrimaryKey);
            }

            public void Remove(int primaryKey)
            {
                if (entities.ContainsKey(primaryKey))
                {
                    entities.Remove(primaryKey);
                }
            }

            public int Count => entities.Count;

            public IReadOnlyList<TEntity> Entities => entities.Values.ToList();

            public int ProjectFilePrimaryKey { get; }
        }

        readonly object indexLock = new object();

        /// <summary>
        /// Where the Key == <see cref="ProjectFileOwnedEntity.ProjectFileKey"/>
        /// </summary>
        readonly Dictionary<int, Index> projectFileKeyIndex = new Dictionary<int, Index>();

        /// <summary>
        /// Where the Key == <see cref="Entity.PrimaryKey"/>
        /// </summary>
        readonly Dictionary<int, Index> primaryKeyIndex = new Dictionary<int, Index>();

        public void Clear()
        {
            lock (indexLock)
            {
                projectFileKeyIndex.Clear();
                primaryKeyIndex.Clear();
            }
        }

        void Remove(int entityPrimaryKey)
        {
            if (primaryKeyIndex.TryGetValue(entityPrimaryKey, out var index))
            {
                primaryKeyIndex.Remove(entityPrimaryKey);
                index.Remove(entityPrimaryKey);

                if (index.Count == 0 && projectFileKeyIndex.ContainsKey(index.ProjectFilePrimaryKey))
                {
                    projectFileKeyIndex.Remove(index.ProjectFilePrimaryKey);
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

        public void OnInserted(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            lock (indexLock)
            {
                var projectFileKey = entity.ProjectFileKey;

                Index entry = null;
                if (!projectFileKeyIndex.TryGetValue(projectFileKey, out entry))
                {
                    entry = new Index(projectFileKey);
                    projectFileKeyIndex[projectFileKey] = entry;
                }

                primaryKeyIndex[entity.PrimaryKey] = entry;
                entry.Add(entity);
            }
        }

        public void OnUpdated(TEntity before, TEntity after)
        {
            if (before.ProjectFileKey != after.ProjectFileKey)
            {
                Remove(before.PrimaryKey);
                OnInserted(after);
            }
        }

        public IReadOnlyList<TEntity> GetForProjectFileKey(int projectFilePrimaryKey)
        {
            if (!projectFileKeyIndex.ContainsKey(projectFilePrimaryKey))
            {
                return new List<TEntity>();
            }

            return projectFileKeyIndex[projectFilePrimaryKey].Entities;
        }
    }
}
