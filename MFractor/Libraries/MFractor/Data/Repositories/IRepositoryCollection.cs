using System;
using System.ComponentModel.Composition;

namespace MFractor.Data.Repositories
{
    /// <summary>
    /// The repository collection interface registers <see cref="IRepository"/> implementations for each entity into a database.
    /// <para/>
    /// Implementations of <see cref="IRepositoryCollection"/> are automatically registered into the <see cref="IResourcesDatabaseEngine"/>.
    /// </summary>
    [InheritedExport]
    public interface IRepositoryCollection
    {
        /// <summary>
        /// Registers all repositories into the provided <paramref name="database"/>.
        /// </summary>
        /// <param name="database">Database.</param>
        void RegisterRepositories(IDatabase database);
	}
}