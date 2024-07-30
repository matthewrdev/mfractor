using System;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Data.Repositories
{
    /// <summary>
    /// The base class for a strongly type entity repository, a data access class that interacts with the resources database.
    /// <para/>
    /// <see cref="EntityRepository{TEntity}"/>'s are strongly typed to <typeparamref name="TEntity"/>, providing overloadable Insert, Update and Delete methods that accept <typeparamref name="TEntity"/>. 
    /// </summary>
    public abstract class EntityRepository<TEntity> : IEntityRepository<TEntity> where TEntity : Entity
    {
        protected EntityRepository(params IEntityIndex<TEntity>[] indexes)
        {
            this.indexes = indexes?.ToList() ?? new List<IEntityIndex<TEntity>>();
        }

        readonly IReadOnlyList<IEntityIndex<TEntity>> indexes;
        public Type EntityType { get; } = typeof(TEntity);

        readonly object entitiesLock = new object();
        readonly Dictionary<int, TEntity> entities = new Dictionary<int, TEntity>();

        readonly object nextPrimaryKeyLock = new object();
        int nextPrimaryKey = 0;

        /// <summary>
        /// Insert the specified entity into the database.
        /// </summary>
        /// <returns>The insert.</returns>
        /// <param name="entity">Entity.</param>
        public int Insert(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.PrimaryKey = IssueNextPrimaryKey();
            Mutate(d => d[entity.PrimaryKey] = entity);

            OnInserted(entity);

            return entity.PrimaryKey;
        }

        protected int IssueNextPrimaryKey()
        {
            lock (nextPrimaryKeyLock)
            {
                nextPrimaryKey++;
                return nextPrimaryKey;
            }
        }

        /// <summary>
        /// Updates the <paramref name="entity"/> in the database.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public void Update(TEntity entity)
        {
            if (entity is null)
            {
                return;
            }

            var before = Get(entity.PrimaryKey);
            if (before is null)
            {
                throw new InvalidOperationException($"Unable to update the entity with the key '{entity.PrimaryKey}' as it does not exist in the repository");
            }

            Mutate(d => d[entity.PrimaryKey] = entity);

            OnUpdated(before, entity);
        }

        /// <summary>
        /// Delete the specified entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public void Delete(TEntity entity)
        {
            if (entity is null)
            {
                return;
            }

            Delete(entity.PrimaryKey);
        }

        public void Delete(IReadOnlyList<TEntity> entities)
        {
            if (entities is null || entities.Count == 0)
            {
                return;
            }

            var keys = entities.Select(e => e.PrimaryKey).ToList();
            DeleteRange(keys);
        }

        public TEntity Get(int primaryKey)
        {
            return Query(data =>
            {
                if (!data.ContainsKey(primaryKey))
                {
                    return null;
                }

                return data[primaryKey];
            });
        }

        protected TResult Query<TResult>(Func<IReadOnlyDictionary<int, TEntity>, TResult> predicate)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            lock (entitiesLock)
            {
                return predicate(entities);
            }
        }

        public IReadOnlyList<TEntity> GetAll()
        {
            return Query(data => data.Values.ToList());
        }

        /// <summary>
        /// Inserts the <paramref name="entity"/> into the database.
        /// </summary>
        /// <returns>The entity.</returns>
        /// <param name="entity">Entity.</param>
        public int InsertEntity(Entity entity)
        {
            if (TryCast(entity, out var result))
            {
                return Insert(result);
            }

            return -1;
        }

        bool TryCast(Entity entity, out TEntity result)
        {
            result = null;
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            result = entity as TEntity;
            if (result == null)
            {
                throw new InvalidOperationException(entity.ToString() + " is not a " + nameof(TEntity));
            }

            return true;
        }

        public void UpdateEntity(Entity entity)
        {
            if (TryCast(entity, out var result))
            {
                Update(result);
            }
        }

        public Entity GetEntity(int primaryKey)
        {
            return Get(primaryKey);
        }

        public IReadOnlyList<Entity> GetAllEntities()
        {
            return GetAll();
        }

        public void Delete(Entity entity)
        {
            if (TryCast(entity, out var result))
            {
                Delete(result);
            }
        }

        public void Delete(int primaryKey)
        {
            Mutate(data =>
            {
                if (data.ContainsKey(primaryKey))
                {
                    data.Remove(primaryKey);
                }
            });

            OnDeleted(primaryKey);
        }

        public void DeleteRange(IReadOnlyList<Entity> entities)
        {
            if (entities == null)
            {
                return;
            }

            var keys = entities.OfType<TEntity>().Select(t => t.PrimaryKey).ToList();

            DeleteRange(keys);
        }

        public void DeleteRange(IReadOnlyList<int> primaryKeys)
        {
            if (primaryKeys is null || !primaryKeys.Any())
            {
                return;
            }

            Mutate(data =>
            {
                foreach (var key in primaryKeys)
                {
                    if (data.ContainsKey(key))
                    {
                        data.Remove(key);
                    }
                }
            });


            OnDeleted(primaryKeys);
        }

        public void DeleteAll()
        {
            Mutate(data => data.Clear());
        }

        protected void Mutate(Action<Dictionary<int, TEntity>> mutator)
        {
            if (mutator is null)
            {
                throw new ArgumentNullException(nameof(mutator));
            }

            lock (entitiesLock)
            {
                mutator(entities);
            }
        }

        public int NextPrimaryKey()
        {
            return IssueNextPrimaryKey();
        }

        void OnInserted(TEntity entity)
        {
            if (indexes.Count == 0)
            {
                return;
            }

            foreach (var index in indexes)
            {
                index.OnInserted(entity);
            }
        }

        void OnUpdated(TEntity before, TEntity after)
        {
            if (indexes.Count == 0)
            {
                return;
            }

            foreach (var index in indexes)
            {
                index.OnUpdated(before, after);
            }
        }

        void OnDeleted(int entityPrimaryKey)
        {
            if (indexes.Count == 0)
            {
                return;
            }

            foreach (var index in indexes)
            {
                index.OnDeleted(entityPrimaryKey);
            }
        }

        void OnDeleted(IReadOnlyList<int> primaryKeys)
        {
            if (indexes.Count == 0)
            {
                return;
            }

            foreach (var index in indexes)
            {
                index.OnDeleted(primaryKeys);
            }
        }

        protected TEntityIndex GetEntityIndex<TEntityIndex>() where TEntityIndex : IEntityIndex<TEntity>
        {
            return indexes.OfType<TEntityIndex>().FirstOrDefault();
        }
    }
}