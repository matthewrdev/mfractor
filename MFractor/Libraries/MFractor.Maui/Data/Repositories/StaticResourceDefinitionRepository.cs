using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Indexes;
using MFractor.Maui.Data.Models;
using MFractor.Workspace.Data.Indexes;
using MFractor.Workspace.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    /// <summary>
    /// The repository that provides data-access for the <see cref="StaticResourceDefinition"/> table.
    /// </summary>
    public class StaticResourceDefinitionRepository : EntityRepository<StaticResourceDefinition>
    {
        public StaticResourceDefinitionRepository()
            : base(new EntityByProjectFileIndex<StaticResourceDefinition>(),
                   new StaticResourcesByNameIndex(),
                   new StyleStaticResourceDefinitionsIndex())
        {
        }

        /// <summary>
        /// Gets the static resources in file.
        /// </summary>
        /// <returns>The static resources in file.</returns>
        /// <param name="file">File.</param>
        public IReadOnlyList<StaticResourceDefinition> GetStaticResourcesInFile(ProjectFile file)
        {
            return GetStaticResourcesInFile(file.PrimaryKey);
        }

        /// <summary>
        /// Gets the static resources in file with the key <paramref name="fileKey"/>.
        /// </summary>
        /// <returns>The static resources in file.</returns>
        /// <param name="fileKey">File key.</param>
        public IReadOnlyList<StaticResourceDefinition> GetStaticResourcesInFile(int fileKey)
        {
            var index = GetEntityIndex<EntityByProjectFileIndex<StaticResourceDefinition>>();

            return index.GetForProjectFileKey(fileKey).Where(pf => !pf.GCMarked).ToList();
        }

        /// <summary>
        /// Gets all static resourcess declaration in <paramref name="file"/> named "<paramref name="name"/>".
        /// </summary>
        /// <returns>The named static resources in file.</returns>
        /// <param name="file">File.</param>
        /// <param name="name">Name.</param>
        public IReadOnlyList<StaticResourceDefinition> GetNamedStaticResourcesInFile(ProjectFile file, string name)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            return GetNamedStaticResourcesInFile(file.PrimaryKey, name);
        }

        /// <summary>
        /// Gets all static resourcess declaration in <paramref name="fileKey"/> named "<paramref name="name"/>".
        /// </summary>
        /// <returns>The named static resources in file.</returns>
        /// <param name="fileKey">File.</param>
        /// <param name="name">Name.</param>
        public IReadOnlyList<StaticResourceDefinition> GetNamedStaticResourcesInFile(int fileKey, string name)
        {
            var index = GetEntityIndex<StaticResourcesByNameIndex>();

            return index.GetNamedStaticResoures(name).Where(entity => entity.ProjectFileKey == fileKey && !entity.GCMarked).ToList();
        }

        /// <summary>
        /// Gets all static resourcess declaration in <paramref name="fileKey"/> named "<paramref name="name"/>".
        /// </summary>
        /// <returns>The named static resources in file.</returns>
        /// <param name="fileKey">File.</param>
        /// <param name="name">Name.</param>
        public IReadOnlyList<StaticResourceDefinition> GetDefinedStyleResources()
        {
            var index = GetEntityIndex<StyleStaticResourceDefinitionsIndex>();

            return index.GetStyleStaticResources().Where(s => !s.GCMarked).ToList();
        }
    }
}
