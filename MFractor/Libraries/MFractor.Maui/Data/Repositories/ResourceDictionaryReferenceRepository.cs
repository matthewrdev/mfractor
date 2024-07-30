using System.Collections.Generic;
using System.Linq;
using MFractor.Data;
using MFractor.Data.Models;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    /// <summary>
    /// The repository that provides data-access for the <see cref="ResourceDictionaryReference"/> table.
    /// </summary>
    public class ResourceDictionaryReferenceRepository : EntityRepository<ResourceDictionaryReference>
    {
        /// <summary>
        /// Gets the resource dictionary references in file.
        /// </summary>
        /// <returns>The resource dictionary references in file.</returns>
        /// <param name="file">File.</param>
        public IReadOnlyList<ResourceDictionaryReference> GetResourceDictionaryReferencesInFile(ProjectFile file)
        {
            return GetResourceDictionaryReferencesInFile(file.PrimaryKey);
        }

        /// <summary>
        /// Gets the resource dictionary references in file.
        /// </summary>
        /// <returns>The resource dictionary references in file.</returns>
        /// <param name="fileKey">File key.</param>
        public IReadOnlyList<ResourceDictionaryReference> GetResourceDictionaryReferencesInFile(int fileKey)
        {
            return Query(data => data.Values.Where(entity => entity.ProjectFileKey == fileKey && !entity.GCMarked).ToList());
        }
    }
}
