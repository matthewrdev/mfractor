using System;
using System.Collections.Generic;
using MFractor.Data.Repositories;

namespace MFractor.Data
{
    /// <summary>
    /// A helper class that provides several extension methods for fluently building entity table mappings and repository mappings.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds a table mapping of <typeparamref name="TEntity"/> to <paramref name="tableName"/> to the <paramref name="dictionary"/>.
        /// <para/>
        /// If no <paramref name="tableName"/> is specified, the type name of the <typeparamref name="TEntity"/> is used.
        /// </summary>
        /// <returns>The table.</returns>
        /// <param name="dictionary">Dictionary.</param>
        /// <param name="tableName">Table name.</param>
        /// <typeparam name="TEntity">The 1st type parameter.</typeparam>
        public static Dictionary<Type, string> AddTable<TEntity>(this Dictionary<Type, string> dictionary, string tableName = "") 
            where TEntity : Entity
        {
            if (dictionary.ContainsKey(typeof(TEntity)))
            {
                throw new InvalidOperationException("A table mapping already exists for " + typeof(TEntity) + ". Tried to use \"" + tableName + "\" but a mapping already exists for + \"" + dictionary[typeof(TEntity)] + "\".");
            }

            var name = string.IsNullOrEmpty(tableName) ? typeof(TEntity).Name : tableName;

            dictionary[typeof(TEntity)] = name;
            return dictionary;
        }

        /// <summary>
        /// Adds a mapping for <typeparamref name="TRepository"/> to the given <paramref name="dictionary"/>.
        /// </summary>
        /// <param name="dictionary">Dictionary.</param>
        /// <param name="repository">Repository.</param>
        /// <typeparam name="TRepository">The 1st type parameter.</typeparam>
        public static void AddRepositoryForType<TRepository>(this Dictionary<Type, IRepository> dictionary, IRepository repository)
        {
            if (dictionary.ContainsKey(typeof(TRepository)))
            {
                throw new InvalidOperationException("A table mapping already exists for " + typeof(TRepository) + ". Tried to use \"" + repository.GetType() + "\" but a mapping already exists for + \"" + dictionary[typeof(TRepository)] + "\".");
            }

            dictionary[typeof(TRepository)] = repository;
        }

        /// <summary>
        /// Checks if the <paramref name="dictionary"/> can resolve the <typeparamref name="TRepository"/>.
        /// </summary>
        /// <returns><c>true</c>, if resolve was caned, <c>false</c> otherwise.</returns>
        /// <param name="dictionary">Dictionary.</param>
        /// <typeparam name="TRepository">The 1st type parameter.</typeparam>
        public static bool CanResolve<TRepository>(this Dictionary<Type, IRepository> dictionary)
        {
            if (dictionary is null)
            {
                return false;
            }

            return dictionary.ContainsKey(typeof(TRepository));
        }

        /// <summary>
        /// Checks if the <paramref name="dictionary"/> can resolve the given <paramref name="type"/>.
        /// </summary>
        /// <returns><c>true</c>, if resolve was caned, <c>false</c> otherwise.</returns>
        /// <param name="dictionary">Dictionary.</param>
        /// <param name="type">Type.</param>
        public static bool CanResolve(this Dictionary<Type, IRepository> dictionary, Type type)
        {
            if (dictionary is null)
            {
                return false;
            }

            if (type is null)
            {
                return false;
            }

            return dictionary.ContainsKey(type);
        }

        /// <summary>
        /// Resolves the repository in the <paramref name="dictionary"/>, cast as <typeparamref name="TRepository"/>.
        /// </summary>
        /// <returns>The resolve.</returns>
        /// <param name="dictionary">Dictionary.</param>
        /// <typeparam name="TRepository">The 1st type parameter.</typeparam>
        public static TRepository Resolve<TRepository>(this Dictionary<Type, IRepository> dictionary)
            where TRepository : class
        {
            if (dictionary is null)
            {
                return null;
            }

            return dictionary.Resolve(typeof(TRepository)) as TRepository;
        }

        /// <summary>
        /// Resolves the repository that matches the <paramref name="type"/> in the <paramref name="dictionary"/>.
        /// </summary>
        /// <returns>The resolve.</returns>
        /// <param name="dictionary">Dictionary.</param>
        /// <param name="type">Type.</param>
        public static IRepository Resolve(this Dictionary<Type, IRepository> dictionary, Type type)
        {
            if (dictionary is null)
            {
                return null;
            }


            if (type is null)
            {
                return null;
            }

            if (!dictionary.ContainsKey(type))
            {
                return null;
            }

            return dictionary[type];
        }
    }
}
