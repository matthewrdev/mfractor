using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Data.Schemas;

namespace MFractor.Data
{
    /// <summary>
    /// An in-memory database that manages schemas and repositories.
    /// </summary>
    public class Database : IDatabase
    {
        /// <summary>
        /// The schemas used by this database.
        /// </summary>
        /// <value>The schema.</value>
        public IReadOnlyList<ISchema> Schemas { get; }

        /// <summary>
        /// The repositories for this database.
        /// </summary>
        /// <value>The repositories.</value>
        public IReadOnlyList<IRepositoryCollection> RepositoryCollections { get; }

        public Database(ISchemaRepository schemaRepository, IRepositoryCollectionRepository repositoryCollectionRepository)
        {
            Schemas = schemaRepository.Schemas;
            RepositoryCollections = repositoryCollectionRepository.RepositoryCollections;

            foreach (var collection in RepositoryCollections)
            {
                collection.RegisterRepositories(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Data.Database"/> class.
        /// </summary>
        /// <param name="schema">Schema.</param>
        /// <param name="repositoryCollection">Repository collection.</param>
        public Database(ISchema schema,
                        IRepositoryCollection repositoryCollection)
        {
            if (schema != null)
            {
                Schemas = new List<ISchema>() { schema };
            }
            else
            {
                Schemas = new List<ISchema>();
            }

            if (repositoryCollection != null)
            {
                RepositoryCollections = new List<IRepositoryCollection>() { repositoryCollection };
            }
            else
            {
                RepositoryCollections = new List<IRepositoryCollection>();
            }

            foreach (var r in RepositoryCollections)
            {
                r.RegisterRepositories(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Data.Database"/> class.
        /// </summary>
        /// <param name="schemas">Schemas.</param>
        /// <param name="repositoryCollection">Repository collections.</param>
        public Database(IEnumerable<ISchema> schemas,
                        IEnumerable<IRepositoryCollection> repositoryCollection)
        {
            Schemas = schemas?.ToList() ?? new List<ISchema>();
            RepositoryCollections = repositoryCollection?.ToList() ?? new List<IRepositoryCollection>();

            foreach (var r in RepositoryCollections)
            {
                r.RegisterRepositories(this);
            }
        }

        /// <summary>
        /// The repositories.
        /// </summary>
        protected Dictionary<Type, IRepository> RegisteredRepositories { get; } = new Dictionary<Type, IRepository>();

        /// <summary>
        /// The repositories for this database
        /// </summary>
        /// <value>The repositories.</value>
        public IReadOnlyList<IRepository> Repositories => RegisteredRepositories.Values.ToList();

        /// <summary>
        /// The mapping of <see cref="Entity"/> to <see cref="IRepository"/> types.
        /// </summary>
        protected Dictionary<Type, Type> EntityRepositoryMap { get; } = new Dictionary<Type, Type>();

        /// <summary>
        /// Registers the repository.
        /// </summary>
        /// <param name="repo">Repo.</param>
        /// <typeparam name="TRepo">The 1st type parameter.</typeparam>
        /// <typeparam name="TEntity">The 2nd type parameter.</typeparam>
        public IDatabase RegisterRepository<TRepo, TEntity>(TRepo repo)
            where TRepo : class, IRepository, IEntityRepository<TEntity>
            where TEntity : Entity, new()
        {
            if (EntityRepositoryMap.ContainsKey(typeof(TEntity)))
            {
                var message = "The database already has a repository mapped to " + typeof(TEntity) + ". Tried to register \"" + typeof(TRepo) + "\" but \"" + EntityRepositoryMap[typeof(TEntity)] + "\" is already mapped.";
                throw new InvalidOperationException(message);
            }

            EntityRepositoryMap[typeof(TEntity)] = typeof(TRepo);

            RegisteredRepositories.AddRepositoryForType<TRepo>(repo);
            RegisteredRepositories.AddRepositoryForType<IEntityRepository<TEntity>>(repo);

            return this;
        }

        /// <summary>
        /// Helper method to return the repository for this database.
        /// </summary>
        /// <returns>The repository.</returns>
        /// <typeparam name="TRepository">The 1st type parameter.</typeparam>
        public TRepository GetRepository<TRepository>() where TRepository : class, IRepository
        {
            if (!RegisteredRepositories.CanResolve<TRepository>())
            {
                return null;
            }

            return RegisteredRepositories.Resolve<TRepository>();
        }

        /// <summary>
        /// Gets the repository for the provided repository type.
        /// </summary>
        /// <returns>The repository.</returns>
        /// <param name="repository">Repository.</param>
        public IRepository GetRepository(Type repository)
        {
            if (!RegisteredRepositories.CanResolve(repository))
            {
                return null;
            }

            return RegisteredRepositories.Resolve(repository) as IRepository;
        }

        /// <summary>
        /// Gets the type of the repository for entity.
        /// </summary>
        /// <returns>The repository for entity type.</returns>
        /// <param name="entityType">Entity type.</param>
        public IRepository GetRepositoryForEntityType(Type entityType)
        {
            if (!EntityRepositoryMap.ContainsKey(entityType))
            {
                return null;
            }

            var entityRepositoryType = EntityRepositoryMap[entityType];

            return GetRepository(entityRepositoryType);
        }

        /// <summary>
        /// Gets the repository for entity.
        /// </summary>
        /// <returns>The repository for entity.</returns>
        /// <typeparam name="TEntity">The 1st type parameter.</typeparam>
        public IEntityRepository<TEntity> GetRepositoryForEntity<TEntity>() where TEntity : Entity, new()
        {
            if (!RegisteredRepositories.CanResolve<IEntityRepository<TEntity>>())
            {
                return null;
            }

            return RegisteredRepositories.Resolve<IEntityRepository<TEntity>>();
        }

        readonly Dictionary<Type, string> tableNameForTypeCache = new Dictionary<Type, string>();

        /// <summary>
        /// Gets the table name for provided type.
        /// </summary>
        /// <returns>The table name for type.</returns>
        /// <param name="type">Type.</param>
        public string GetTableNameForType(Type type)
        {
            if (tableNameForTypeCache.ContainsKey(type))
            {
                return tableNameForTypeCache[type];
            }

            foreach (var schema in Schemas)
            {
                if (schema.HasTableFor(type))
                {
                    var tableName = schema.GetTableName(type);

                    tableNameForTypeCache[type] = tableName;

                    return tableName;
                }
            }

            return string.Empty;
        }
    }
}
