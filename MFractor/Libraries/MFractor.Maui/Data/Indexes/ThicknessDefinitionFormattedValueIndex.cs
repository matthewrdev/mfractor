using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Indexes
{
    public class ThicknessDefinitionFormattedValueIndex : IEntityIndex<ThicknessDefinition>
    {
        class Index
        {
            public Index(string formattedThicknessValue)
            {
                FormattedThicknessValue = formattedThicknessValue;
            }

            readonly Dictionary<int, ThicknessDefinition> entities = new Dictionary<int, ThicknessDefinition>();

            public void Add(ThicknessDefinition entity)
            {
                if (entity is null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                entities[entity.PrimaryKey] = entity;
            }

            public void Remove(ThicknessDefinition entity)
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

            public IReadOnlyList<ThicknessDefinition> Entities => entities.Values.ToList();

            public string FormattedThicknessValue { get; }
        }

        readonly object indexLock = new object();

        /// <summary>
        /// Where the Key == <see cref="ThicknessDefinition.FormattedValue"/>
        /// </summary>
        readonly Dictionary<string, Index> thicknessFormattedValueIndex = new Dictionary<string, Index>();

        /// <summary>
        /// Where the Key == <see cref="ThicknessDefinition.PrimaryKey"/>
        /// </summary>
        readonly Dictionary<int, Index> primaryKeyIndex = new Dictionary<int, Index>();

        public void Clear()
        {
            lock (indexLock)
            {
                thicknessFormattedValueIndex.Clear();
                primaryKeyIndex.Clear();
            }
        }

        void Remove(int entityPrimaryKey)
        {
            if (primaryKeyIndex.TryGetValue(entityPrimaryKey, out var index))
            {
                primaryKeyIndex.Remove(entityPrimaryKey);
                index.Remove(entityPrimaryKey);

                if (index.Count == 0 && thicknessFormattedValueIndex.ContainsKey(index.FormattedThicknessValue))
                {
                    thicknessFormattedValueIndex.Remove(index.FormattedThicknessValue);
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

        public void OnInserted(ThicknessDefinition entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            lock (indexLock)
            {
                var formattedValue = entity.FormattedValue;

                Index entry = null;
                if (!thicknessFormattedValueIndex.TryGetValue(formattedValue, out entry))
                {
                    entry = new Index(formattedValue);
                    thicknessFormattedValueIndex[formattedValue] = entry;
                }

                primaryKeyIndex[entity.PrimaryKey] = entry;
                entry.Add(entity);
            }
        }

        public void OnUpdated(ThicknessDefinition before, ThicknessDefinition after)
        {
            if (before.FormattedValue != after.FormattedValue)
            {
                Remove(before.PrimaryKey);
                OnInserted(after);
            }
        }

        public IReadOnlyList<ThicknessDefinition> GetForFormattedValue(string formattedValue)
        {
            if (!thicknessFormattedValueIndex.ContainsKey(formattedValue))
            {
                return new List<ThicknessDefinition>();
            }

            return thicknessFormattedValueIndex[formattedValue].Entities;
        }
    }
}
