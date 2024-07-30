using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Indexes
{
    public class StyleStaticResourceDefinitionsIndex : IEntityIndex<StaticResourceDefinition>
    {
        readonly object indexLock = new object();

        /// <summary>
        /// Where the Key == <see cref="StyleSetter.PrimaryKey"/>
        /// </summary>
        readonly Dictionary<int, StaticResourceDefinition> index = new Dictionary<int, StaticResourceDefinition>();

        public void Clear()
        {
            lock (indexLock)
            {
                index.Clear();
            }
        }

        void Remove(int entityPrimaryKey)
        {
            if (index.ContainsKey(entityPrimaryKey))
            {
                index.Remove(entityPrimaryKey);
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

            lock (indexLock)
            {
                index[entity.PrimaryKey] = entity;
            }
        }

        public void OnUpdated(StaticResourceDefinition before, StaticResourceDefinition after)
        {
            if (before.IsStyleMetaType)
            {
                Remove(before.PrimaryKey);
            }    

            if (after.IsStyleMetaType)
            {
                OnInserted(after);
            }
        }

        public IReadOnlyList<StaticResourceDefinition> GetStyleStaticResources()
        {
            return index.Values.ToList();
        }
    }
}