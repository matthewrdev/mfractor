using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Indexes
{
    public class ColorUsageColorIntegerIndex : IEntityIndex<ColorUsage>
    {
        class Index
        {
            public Index(int colorInteger)
            {
                ColorInteger = colorInteger;
            }

            readonly Dictionary<int, ColorUsage> entities = new Dictionary<int, ColorUsage>();

            public void Add(ColorUsage entity)
            {
                if (entity is null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                entities[entity.PrimaryKey] = entity;
            }

            public void Remove(ColorUsage entity)
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

            public IReadOnlyList<ColorUsage> ColorUsages => entities.Values.ToList();

            public int ColorInteger { get; }
        }

        readonly object indexLock = new object();

        /// <summary>
        /// Where the Key == <see cref="ColorUsage.ColorInteger"/>
        /// </summary>
        readonly Dictionary<int, Index> colorIntegerIndex = new Dictionary<int, Index>();

        /// <summary>
        /// Where the Key == <see cref="ColorUsage.PrimaryKey"/>
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

        public void OnInserted(ColorUsage entity)
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

        public void OnUpdated(ColorUsage before, ColorUsage after)
        {
            if (before.ColorInteger != after.ColorInteger)
            {
                Remove(before.PrimaryKey);
                OnInserted(after);
            }
        }

        public IReadOnlyList<ColorUsage> GetForColorInteger(int colorInteger)
        {
            if (!colorIntegerIndex.ContainsKey(colorInteger))
            {
                return new List<ColorUsage>();
            }

            return colorIntegerIndex[colorInteger].ColorUsages;
        }
    }
}