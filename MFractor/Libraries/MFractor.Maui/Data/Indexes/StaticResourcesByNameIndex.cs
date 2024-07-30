using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Indexes
{
    public class StaticResourcesByNameIndex : IEntityIndex<StaticResourceDefinition>
    {
        class Index
        {
            public Index(string resourceName)
            {
                ResourceName = resourceName;
            }

            readonly Dictionary<int, StaticResourceDefinition> entities = new Dictionary<int, StaticResourceDefinition>();

            public void Add(StaticResourceDefinition entity)
            {
                if (entity is null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                entities[entity.PrimaryKey] = entity;
            }

            public void Remove(StaticResourceDefinition entity)
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

            public IReadOnlyList<StaticResourceDefinition> Entities => entities.Values.ToList();

            public string ResourceName { get; }
        }

        readonly object indexLock = new object();

        /// <summary>
        /// Where the Key == <see cref="StaticResourceDefinition.Name"/>
        /// </summary>
        readonly Dictionary<string, Index> staticResourceNameIndex = new Dictionary<string, Index>();

        /// <summary>
        /// Where the Key == <see cref="StaticResourceDefinition.PrimaryKey"/>
        /// </summary>
        readonly Dictionary<int, Index> primaryKeyIndex = new Dictionary<int, Index>();

        public void Clear()
        {
            lock (indexLock)
            {
                staticResourceNameIndex.Clear();
                primaryKeyIndex.Clear();
            }
        }

        void Remove(int entityPrimaryKey)
        {
            if (primaryKeyIndex.TryGetValue(entityPrimaryKey, out var index))
            {
                primaryKeyIndex.Remove(entityPrimaryKey);
                index.Remove(entityPrimaryKey);

                if (index.Count == 0
                    && staticResourceNameIndex.ContainsKey(index.ResourceName))
                {
                    staticResourceNameIndex.Remove(index.ResourceName);
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

        public void OnInserted(StaticResourceDefinition entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.Name))
            {
                return;
            }

            lock (indexLock)
            {
                var resourceName = entity.Name;

                Index entry = null;
                if (!staticResourceNameIndex.TryGetValue(resourceName, out entry))
                {
                    entry = new Index(resourceName);
                    staticResourceNameIndex[resourceName] = entry;
                }

                primaryKeyIndex[entity.PrimaryKey] = entry;
                entry.Add(entity);
            }
        }

        public void OnUpdated(StaticResourceDefinition before, StaticResourceDefinition after)
        {
            if (before.Name != after.Name)
            {
                Remove(before.PrimaryKey);
                OnInserted(after);
            }
        }

        public IReadOnlyList<StaticResourceDefinition> GetNamedStaticResoures(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName) || !staticResourceNameIndex.ContainsKey(resourceName))
            {
                return new List<StaticResourceDefinition>();
            }

            return staticResourceNameIndex[resourceName].Entities;
        }
    }
}
