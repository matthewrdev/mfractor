using System;
using System.Collections.Generic;

namespace MFractor.Data.Repositories
{
    /// <summary>
    /// A strongly typed <see cref="IRepository"/> for working with an <typeparamref name="TEntity"/> against a database.
    /// </summary>
    public interface IEntityRepository<TEntity> : IRepository where TEntity : Entity
    {
        /// <summary>
        /// Inserts the entity into the database.
        /// </summary>
        /// <returns>The insert.</returns>
        /// <param name="entity">Entity.</param>
        int Insert(TEntity entity);

        /// <summary>
        /// Updates the given <paramref name="entity"/> in the database.
        /// </summary>
        /// <param name="entity">Entity.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Gets the <typeparamref name="TEntity"/> instance that has the provided <paramref name="primaryKey"/>.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="primaryKey">Primary key.</param>
        TEntity Get(int primaryKey);

        /// <summary>
        /// Gets all <typeparamref name="TEntity"/>'s from the database.
        /// </summary>
        /// <returns>The all.</returns>
        IReadOnlyList<TEntity> GetAll();

        /// <summary>
        /// Deletes the given <paramref name="entity"/> from the database.
        /// </summary>
        /// <param name="entity">Entity.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Deletes all of the given <paramref name="entities"/> from the database.
        /// </summary>
        /// <param name="entities">Entities.</param>
        void Delete(IReadOnlyList<TEntity> entities);
    }
}
