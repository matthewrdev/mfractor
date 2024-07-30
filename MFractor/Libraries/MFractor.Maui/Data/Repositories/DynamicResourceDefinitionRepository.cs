using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Models;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Data.Repositories
{
    /// <summary>
    /// The repository that provides data-access for the <see cref="DynamicResourceDefinition"/> table.
    /// </summary>
    public class DynamicResourceDefinitionRepository : EntityRepository<DynamicResourceDefinition>
    {
        public List<DynamicResourceDefinition> GetNamedDynamicResources(string resourceName)
        {
            return Query(data => data.Values.Where(entity => entity.Name == resourceName && !entity.GCMarked).ToList());
        }

        /// <summary>
        /// Does the <paramref name="namedType"/> declared any dynamic resources?
        /// </summary>
        /// <returns><c>true</c>, if dynamic resources was hased, <c>false</c> otherwise.</returns>
        /// <param name="namedType">Named type.</param>
        public bool HasDynamicResources(INamedTypeSymbol namedType)
        {
            if (namedType == null)
            {
                return false;
            }

            return HasDynamicResources(namedType.ToString());
        }

        /// <summary>
        /// Does the fully qualified <paramref name="symbolName"/> declared any dynamic resources?
        /// </summary>
        /// <returns><c>true</c>, if dynamic resources was hased, <c>false</c> otherwise.</returns>
        /// <param name="symbolName">Named type.</param>
        public bool HasDynamicResources(string symbolName)
        {
            return Query(data => data.Values.Any(entity => entity.OwnerSymbolMetaType == symbolName && !entity.GCMarked));
        }

        /// <summary>
        /// Gets the static resources in file.
        /// </summary>
        /// <returns>The static resources in file.</returns>
        /// <param name="file">File.</param>
        public List<DynamicResourceDefinition> GetDynamicResourcesInFile(ProjectFile file)
        {
            return GetDynamicResourcesInFile(file.PrimaryKey);
        }

        /// <summary>
        /// Gets the static resources in file with the key <paramref name="fileKey"/>.
        /// </summary>
        /// <returns>The static resources in file.</returns>
        /// <param name="fileKey">File key.</param>
        public List<DynamicResourceDefinition> GetDynamicResourcesInFile(int fileKey)
        {
            return Query(data => data.Values.Where(entity => entity.ProjectFileKey == fileKey && !entity.GCMarked).ToList());
        }

        /// <summary>
        /// For the given <paramref name="namedType"/>, returns the dynamic resources that the type declares.
        /// </summary>
        /// <returns>The dynamic resources for symbol.</returns>
        /// <param name="namedType">The named type.</param>
        public List<DynamicResourceDefinition> GetDynamicResourcesDeclaredBySymbol(INamedTypeSymbol namedType)
        {
            if (namedType == null)
            {
                return new List<DynamicResourceDefinition>();
            }

            return GetDynamicResourcesDeclaredBySymbol(namedType.ToString());
        }

        /// <summary>
        /// For the given fully qualified <paramref name="symbolName"/>, returns the dynamic resources that the symbol declares.
        /// </summary>
        /// <returns>The dynamic resources for symbol.</returns>
        /// <param name="symbolName">Symbol name.</param>
        public List<DynamicResourceDefinition> GetDynamicResourcesDeclaredBySymbol(string symbolName)
        {
            return Query(data => data.Values.Where(entity => entity.OwnerSymbolMetaType == symbolName && !entity.GCMarked).ToList());
        }

        /// <summary>
        /// Gets all static resourcess declaration in <paramref name="file"/> named "<paramref name="file"/>".
        /// </summary>
        /// <returns>The named static resources in file.</returns>
        /// <param name="file">File.</param>
        /// <param name="name">Name.</param>
        public List<DynamicResourceDefinition> GetNamedDynamicResourcesInFile(ProjectFile file, string name)
        {
            return GetNamedDynamicResourcesInFile(file.PrimaryKey, name);
        }

        /// <summary>
        /// Gets all static resourcess declaration in <paramref name="fileKey"/> named "<paramref name="name"/>".
        /// </summary>
        /// <returns>The named static resources in file.</returns>
        /// <param name="fileKey">File.</param>
        /// <param name="name">Name.</param>
        public List<DynamicResourceDefinition> GetNamedDynamicResourcesInFile(int fileKey, string name)
        {
            return Query(data => data.Values.Where(entity => entity.ProjectFileKey == fileKey && entity.Name == name && !entity.GCMarked).ToList());
        }


        /// <summary>
        /// For the given <paramref name="namedType"/>, returns the dynamic resources that the symbol declares where the <see cref="DynamicResourceDefinition.Name"/> is <paramref name="resourceName"/>.
        /// </summary>
        /// <returns>The dynamic resources for symbol.</returns>
        /// <param name="namedType">Symbol name.</param>
        public List<DynamicResourceDefinition> GetNamedDynamicResourcesDeclaredBySymbol(INamedTypeSymbol namedType, string resourceName)
        {
            if (namedType == null)
            {
                return new List<DynamicResourceDefinition>();
            }

            return GetNamedDynamicResourcesDeclaredBySymbol(namedType.ToString(), resourceName);
        }

        /// <summary>
        /// For the given fully qualified <paramref name="symbolName"/>, returns the dynamic resources that the symbol declares where the <see cref="DynamicResourceDefinition.Name"/> is <paramref name="resourceName"/>.
        /// </summary>
        /// <returns>The dynamic resources for symbol.</returns>
        /// <param name="symbolName">Symbol name.</param>
        public List<DynamicResourceDefinition> GetNamedDynamicResourcesDeclaredBySymbol(string symbolName, string resourceName)
        {
            return Query(data => data.Values.Where(entity => entity.OwnerSymbolMetaType == symbolName && entity.Name == resourceName  && !entity.GCMarked).ToList());
        }
    }
}
