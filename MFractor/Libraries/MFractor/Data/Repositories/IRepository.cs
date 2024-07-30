using System;
using System.Collections.Generic;

namespace MFractor.Data.Repositories
{
    /// <summary>
    /// A data-access contract for working with database entities.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// The <see cref="Type"/> of the entity that this rep
        /// </summary>
        /// <value>The type of the entity.</value>
        Type EntityType { get; }

        /// <summary>
        /// Inserts the <paramref name="entity"/> into the database.
        /// </summary>
        /// <returns>The entity.</returns>
        /// <param name="entity">Entity.</param>
        int InsertEntity(Entity entity);

        /// <summary>
        /// Updates the given <paramref name="entity"/> in the database.
        /// </summary>
        /// <param name="entity">Entity.</param>
        void UpdateEntity(Entity entity);

        /// <summary>
        /// Gets the entity with the <paramref name="primaryKey"/>
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        Entity GetEntity(int primaryKey);

        /// <summary>
        /// Gets all the entities within the <see cref="Table"/>
        /// </summary>
        /// <returns>The all entities.</returns>
        IReadOnlyList<Entity> GetAllEntities();

        /// <summary>
        /// Delets the given <paramref name="entity"/> from the database.
        /// </summary>
        /// <param name="entity">Entity.</param>
        void Delete(Entity entity);

        /// <summary>
        /// Deletes the entity with the <paramref name="primaryKey"/> from the <see cref="Table"/>.
        /// </summary>
        /// <param name="primaryKey">Primary key.</param>
        void Delete(int primaryKey);

        /// <summary>
        /// Deletes the <paramref name="entities"/>
        /// </summary>
        /// <param name="entities"></param>
        void DeleteRange(IReadOnlyList<Entity> entities);

        /// <summary>
        /// Deletes the entities with the given <paramref name="primaryKeys"/>
        /// </summary>
        /// <param name="primaryKeys"></param>
        void DeleteRange(IReadOnlyList<int> primaryKeys);

        /// <summary>
        /// Deletes all entities in this table.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Inspect what the next primary key will be.
        /// </summary>
        /// <returns></returns>
        int NextPrimaryKey();
    }
}
