using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Indexes
{
    public class ColorDefinitionsByColorIndex : IEntityIndex<ColorDefinition>
    {
        class Index
        {
            public Index(int colorInteger)
            {
                ColorInteger = colorInteger;
            }

            readonly Dictionary<int, ColorDefinition> entities = new Dictionary<int, ColorDefinition>();

            public void Add(ColorDefinition entity)
            {
                if (entity is null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                entities[entity.PrimaryKey] = entity;
            }

            public void Remove(ColorDefinition entity)
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

            public IReadOnlyList<ColorDefinition> Entities => entities.Values.ToList();

            public int ColorInteger { get; }
        }

        readonly object indexLock = new object();

        /// <summary>
        /// Where the Key == <see cref="ColorDefinition.ColorInteger"/>
        /// </summary>
        readonly Dictionary<int, Index> colorIntegerIndex = new Dictionary<int, Index>();

        /// <summary>
        /// Where the Key == <see cref="ColorDefinition.PrimaryKey"/>
        /// </summary>
        readonly Dictionary<int, Index> primaryKeyIndex = new Dictionary<int, Index>();

        public void Clear()
        {
            lock (indexLock)
            {
                colorIntegerIndex.Clear();
                primaryKeyIndex.Clear();
            }
        }

        void Remove(int entityPrimaryKey)
        {
            if (primaryKeyIndex.TryGetValue(entityPrimaryKey, out var index))
            {
                primaryKeyIndex.Remove(entityPrimaryKey);
                index.Remove(entityPrimaryKey);

                if (index.Count == 0 && colorIntegerIndex.ContainsKey(index.ColorInteger))
                {
                    colorIntegerIndex.Remove(index.ColorInteger);
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

        public void OnInserted(ColorDefinition entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            lock (indexLock)
            {
                var colorInteger = entity.ColorInteger;

                Index entry = null;
                if (!colorIntegerIndex.TryGetValue(colorInteger, out entry))
                {
                    entry = new Index(colorInteger);
                    colorIntegerIndex[colorInteger] = entry;
                }

                primaryKeyIndex[entity.PrimaryKey] = entry;
                entry.Add(entity);
            }
        }

        public void OnUpdated(ColorDefinition before, ColorDefinition after)
        {
            if (before.ColorInteger != after.ColorInteger)
            {
                Remove(before.PrimaryKey);
                OnInserted(after);
            }
        }

        public IReadOnlyList<ColorDefinition> GetForColorInteger(int colorInteger)
        {
            if (!colorIntegerIndex.ContainsKey(colorInteger))
            {
                return new List<ColorDefinition>();
            }

            return colorIntegerIndex[colorInteger].Entities;
        }
    }
}