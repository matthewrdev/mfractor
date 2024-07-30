using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MFractor.Data.Schemas
{
    /// <summary>
    /// Defines a set of tables to add to a <see cref="IDatabase"/>.
    /// <para/>
    /// Implementations of <see cref="ISchema"/> are automatically registered into the <see cref="IResourcesDatabaseEngine"/>.
    /// </summary>
    [InheritedExport]
    public interface ISchema
    {
        /// <summary>
        /// The domain of this schema.
        /// <para/>
        /// The domain is prefixed onto all table names to ensure they are unique across the product.
        /// <para/>
        /// For example, XAML platforms and Android both have the concept of Styles in XAML and AXML. We may ended up naming a model 'StyleDefinition' in both schemas, causing a table name conflict. Using a different domain means each feature suite can 
        /// </summary>
        string Domain { get; }

        /// <summary>
        /// The mapping of <see cref="Entity"/> types to table names.
        /// </summary>
        /// <value>The tables.</value>
        IReadOnlyDictionary<Type, string> Tables { get; }

        /// <summary>
        /// The entity types provided by this <see cref="ISchema"/>.
        /// </summary>
        /// <value>The tables.</value>
        IReadOnlyList<Type> Entities { get; }

        /// <summary>
        /// Does this schema have a table defined for <paramref name="type"/>?
        /// </summary>
        /// <returns><c>true</c>, if table for was hased, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        bool HasTableFor(Type type);

        /// <summary>
        /// Does this schema have a table defined for <typeparamref name="TEntity"/>?
        /// </summary>
        /// <returns><c>true</c>, if table for was hased, <c>false</c> otherwise.</returns>
        /// <typeparam name="TEntity">The 1st type parameter.</typeparam>
        bool HasTableFor<TEntity>() where TEntity : Entity;

        /// <summary>
        /// Gets the <see cref="Type"/> that is mapped to that <paramref name="table"/>.
        /// </summary>
        /// <returns>The type for table.</returns>
        /// <param name="table">Table.</param>
        Type GetTypeForTable(string table);

        /// <summary>
        /// Gets the table name that is mapped to the given <paramref name="type"/>.
        /// </summary>
        /// <returns>The table name.</returns>
        /// <param name="type">Type.</param>
        string GetTableName(Type type);

        /// <summary>
        /// Gets the table name that is mapped to the given <typeparamref name="TEntity"/> type.
        /// </summary>
        /// <returns>The table name.</returns>
        /// <typeparam name="TEntity">The 1st type parameter.</typeparam>
        string GetTableName<TEntity>() where TEntity : Entity;
    }
}