using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Indexes
{
    public class StyleDefinitionStaticResourceIndex : IEntityIndex<StyleDefinition>
    {
        class Index
        {
            public Index(int staticResourceKey)
            {
                StaticResourceKey = staticResourceKey;
            }

            readonly Dictionary<int, StyleDefinition> entities = new Dictionary<int, StyleDefinition>();

            public void Add(StyleDefinition entity)
            {
                if (entity is null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                entities[entity.PrimaryKey] = entity;
            }

            public void Remove(StyleSetter entity)
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

            public IReadOnlyList<StyleDefinition> Entities => entities.Values.ToList();

            public int StaticResourceKey { get; }
        }

        readonly object indexLock = new object();

        /// <summary>
        /// Where the Key == <see cref="StyleDefinition.StaticResourceId"/>
        /// </summary>
        readonly Dictionary<int, Index> staticResourceIndex = new Dictionary<int, Index>();

        /// <summary>
        /// Where the Key == <see cref="StyleDefinition.PrimaryKey"/>
        /// </summary>
        readonly Dictionary<int, Index> primaryKeyIndex = new Dictionary<int, Index>();

        public void Clear()
        {
            lock (indexLock)
            {
                staticResourceIndex.Clear();
                primaryKeyIndex.Clear();
            }
        }

        void Remove(int entityPrimaryKey)
        {
            if (primaryKeyIndex.TryGetValue(entityPrimaryKey, out var index))
            {
                primaryKeyIndex.Remove(entityPrimaryKey);
                index.Remove(entityPrimaryKey);

                if (index.Count == 0 && staticResourceIndex.ContainsKey(index.StaticResourceKey))
                {
                    staticResourceIndex.Remove(index.StaticResourceKey);
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

        public void OnInserted(StyleDefinition entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            lock (indexLock)
            {
                var staticResourceKey = entity.StaticResourceId;

                Index entry = null;
                if (!staticResourceIndex.TryGetValue(staticResourceKey, out entry))
                {
                    entry = new Index(staticResourceKey);
                    staticResourceIndex[staticResourceKey] = entry;
                }

                primaryKeyIndex[entity.PrimaryKey] = entry;
                entry.Add(entity);
            }
        }

        public void OnUpdated(StyleDefinition before, StyleDefinition after)
        {
            if (before.StaticResourceId != after.StaticResourceId)
            {
                Remove(before.PrimaryKey);
                OnInserted(after);
            }
        }

        public IReadOnlyList<StyleDefinition> GetStyleDefinitionsForStaticResourceKey(int staticResourceKey)
        {
            if (!staticResourceIndex.ContainsKey(staticResourceKey))
            {
                return new List<StyleDefinition>();
            }

            return staticResourceIndex[staticResourceKey].Entities;
        }
    }
}