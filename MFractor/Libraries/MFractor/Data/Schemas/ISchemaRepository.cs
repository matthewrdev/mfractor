using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Data.Schemas
{
    /// <summary>
    /// Manages implementations of <see cref="ISchema"/> and allows access to them.
    /// </summary>
    public interface ISchemaRepository : IPartRepository<ISchema>
    {
        /// <summary>
        /// The database schemas available in the current application.
        /// </summary>
        IReadOnlyList<ISchema> Schemas { get; }

        /// <summary>
        /// Gets the table name for the given <paramref name="entityType"/>/
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        string GetTableName(Type entityType);

        /// <summary>
        /// Gets the table name for the given <typeparamref name="TEntity"/>/
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        string GetTableName<TEntity>();
    }
}
