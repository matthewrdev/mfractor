using System;
using System.Collections.Generic;
using MFractor.Data.Repositories;
using MFractor.Data.Schemas;

namespace MFractor.Data
{
    /// <summary>
    /// A database.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// The <see cref="ISchema"/>'s used by this database.
        /// </summary>
        /// <value>The schema.</value>
        IReadOnlyList<ISchema> Schemas { get; }

        /// <summary>
        /// The <see cref="IRepositoryCollection"/>'s for this database.
        /// </summary>
        /// <value>The repository providers.</value>
        IReadOnlyList<IRepositoryCollection> RepositoryCollections { get; }

        /// <summary>
        /// The repositories for this database
        /// </summary>
        /// <value>The repositories.</value>
        IReadOnlyList<IRepository> Repositories { get; }

        /// <summary>
        /// Registers the repository.
        /// </summary>
        /// <param name="repo">Repo.</param>
        /// <typeparam name="TRepo">The 1st type parameter.</typeparam>
        /// <typeparam name="TEntity">The 2nd type parameter.</typeparam>
        IDatabase RegisterRepository<TRepo, TEntity>(TRepo repo)
                    where TRepo : class, IRepository, IEntityRepository<TEntity>
                    where TEntity : Entity, new();

        /// <summary>
        /// Helper method to return the repository for this database.
        /// </summary>
        /// <returns>The repository.</returns>
        /// <typeparam name="TRepository">The 1st type parameter.</typeparam>
        TRepository GetRepository<TRepository>() where TRepository : class, IRepository;

        /// <summary>
        /// Gets the repository for the provided repository type.
        /// </summary>
        /// <returns>The repository.</returns>
        /// <param name="repository">Repository.</param>
        IRepository GetRepository(Type repository);

        /// <summary>
        /// Gets the type of the repository for entity.
        /// </summary>
        /// <returns>The repository for entity type.</returns>
        /// <param name="entityType">Entity type.</param>
        IRepository GetRepositoryForEntityType(Type entityType);

        /// <summary>
        /// Gets the repository for entity.
        /// </summary>
        /// <returns>The repository for entity.</returns>
        /// <typeparam name="TEntity">The 1st type parameter.</typeparam>
        IEntityRepository<TEntity> GetRepositoryForEntity<TEntity>() where TEntity : Entity, new();
    }
}

