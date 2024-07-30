using System;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Data.Schemas
{
    /// <summary>
    /// The abstract base class for a new schema addition to an <see cref="IDatabase"/>.
    /// </summary>
	public abstract class Schema : ISchema
	{
        IReadOnlyDictionary<Type, string> tables;

        /// <summary>
        /// The tables provided by this schema.
        /// </summary>
        /// <value>The tables.</value>
        public IReadOnlyDictionary<Type, string> Tables
        {
            get
            {
                if (tables == null)
                {
                    tables = BuildTables() ?? new Dictionary<Type, string>();
                }

                return tables;
            }
        }

        /// <summary>
        /// The entity types provided by this <see cref="ISchema"/>.
        /// </summary>
        /// <value>The tables.</value>
        public IReadOnlyList<Type> Entities => Tables.Keys.ToList();

        public abstract string Domain { get; }


        /// <summary>
        /// Builds the tables.
        /// </summary>
        /// <returns>The tables.</returns>
        protected abstract IReadOnlyDictionary<Type, string> BuildTables();

        /// <summary>
        /// Does this schema have a table defined for <paramref name="type"/>?
        /// </summary>
        /// <returns><c>true</c>, if table for was hased, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        public bool HasTableFor(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return Tables.ContainsKey(type);
        }

        /// <summary>
        /// Does this schema have a table defined for <typeparamref name="TEntity"/>?
        /// </summary>
        /// <returns><c>true</c>, if table for was hased, <c>false</c> otherwise.</returns>
        /// <typeparam name="TEntity">The 1st type parameter.</typeparam>
        public bool HasTableFor<TEntity>() where TEntity : Entity
        {
            return HasTableFor(typeof(TEntity));
        }

        /// <summary>
        /// Gets the table name that is mapped to the given <paramref name="type"/>.
        /// </summary>
        /// <returns>The table name.</returns>
        /// <param name="type">Type.</param>
        public string GetTableName(Type type)
        {
            if (!HasTableFor(type))
            {
                return string.Empty;
            }

            var formattedDomain = string.Empty;

            if (!string.IsNullOrEmpty(Domain))
            {
                formattedDomain = Domain + "_";
            }

            return formattedDomain + Tables[type];
        }

        /// <summary>
        /// Gets the table name that is mapped to the given <typeparamref name="TEntity"/> type.
        /// </summary>
        /// <returns>The table name.</returns>
        /// <typeparam name="TEntity">The 1st type parameter.</typeparam>
        public string GetTableName<TEntity>() where TEntity : Entity
        {
            var type = typeof(TEntity);

            return GetTableName(type);
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/> that is mapped to that <paramref name="table"/>.
        /// </summary>
        /// <returns>The type for table.</returns>
        /// <param name="table">Table.</param>
        public Type GetTypeForTable(string table)
        {
            foreach (var pair in Tables)
            {
                if (pair.Value == table)
                {
                    return pair.Key;
                }
            }

            return null;
        }
    }
}

