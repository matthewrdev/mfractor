
using MFractor.Data.Models;

namespace MFractor.Workspace.Data.Models
{
    /// <summary>
    /// An entity that is owned by a project file.
    /// <para/>
    /// As <see cref="ProjectFileOwnedEntity"/> derives from <see cref="GCEntity"/>, all children of the parent project file will
    /// automatically have <see cref="GCEntity.GCMarked"/> set to true when the owning <see cref="ProjectFile"/> is marked.
    /// </summary>
    public abstract class ProjectFileOwnedEntity : GCEntity 
    {
        /// <summary>
        /// The primary key of the project file that owns this entity.
        /// </summary>
        /// <value>The file key.</value>
        public int ProjectFileKey { get; set; }
    }
}
