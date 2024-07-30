using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Indexes
{
    public class StyleSetterStyleDefinitionIndex : IEntityIndex<StyleSetter>
    {
        class Index
        {
            public Index(int styleDefinitionKey)
            {
                StyleDefinitionKey = styleDefinitionKey;
            }

            readonly Dictionary<int, StyleSetter> entities = new Dictionary<int, StyleSetter>();

            public void Add(StyleSetter entity)
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

            public IReadOnlyList<StyleSetter> Entities => entities.Values.ToList();

            public int StyleDefinitionKey { get; }
        }

        readonly object indexLock = new object();

        /// <summary>
        /// Where the Key == <see cref="StyleSetter.{"/>
        /// </summary>
        readonly Dictionary<int, Index> styleDefinitionKeyIndex = new Dictionary<int, Index>();

        /// <summary>
        /// Where the Key == <see cref="StyleSetter.PrimaryKey"/>
        /// </summary>
        readonly Dictionary<int, Index> primaryKeyIndex = new Dictionary<int, Index>();

        public void Clear()
        {
            lock (indexLock)
            {
                styleDefinitionKeyIndex.Clear();
                primaryKeyIndex.Clear();
            }
        }

        void Remove(int entityPrimaryKey)
        {
            if (primaryKeyIndex.TryGetValue(entityPrimaryKey, out var index))
            {
                primaryKeyIndex.Remove(entityPrimaryKey);
                index.Remove(entityPrimaryKey);

                if (index.Count == 0 && styleDefinitionKeyIndex.ContainsKey(index.StyleDefinitionKey))
                {
                    styleDefinitionKeyIndex.Remove(index.StyleDefinitionKey);
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

        public void OnInserted(StyleSetter entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            lock (indexLock)
            {
                var styleDefinitionKey = entity.StyleDefinitionKey;

                Index entry = null;
                if (!styleDefinitionKeyIndex.TryGetValue(styleDefinitionKey, out entry))
                {
                    entry = new Index(styleDefinitionKey);
                    styleDefinitionKeyIndex[styleDefinitionKey] = entry;
                }

                primaryKeyIndex[entity.PrimaryKey] = entry;
                entry.Add(entity);
            }
        }

        public void OnUpdated(StyleSetter before, StyleSetter after)
        {
            if (before.StyleDefinitionKey != after.StyleDefinitionKey)
            {
                Remove(before.PrimaryKey);
                OnInserted(after);
            }
        }

        public IReadOnlyList<StyleSetter> GetSettersForStyleDefinitionKey(int styleDefinitionKey)
        {
            if (!styleDefinitionKeyIndex.ContainsKey(styleDefinitionKey))
            {
                return new List<StyleSetter>();
            }

            return styleDefinitionKeyIndex[styleDefinitionKey].Entities;
        }
    }
}