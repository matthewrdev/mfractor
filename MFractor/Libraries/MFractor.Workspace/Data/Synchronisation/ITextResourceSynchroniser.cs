using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Text;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace.Data.Synchronisation
{
    /// <summary>
    /// A resource synchroniser that consumes text files through an <see cref="ITextProvider"/>.
    /// </summary>
    [InheritedExport]
    public interface ITextResourceSynchroniser
    {
        /// <summary>
        /// The file extensions that this synchroniser supports.
        /// <para/>
        /// Each extension should include the '.'. 
        /// <para/>
        /// For example: { ".resx", ".xml", ".axml" }
        /// </summary>
        string[] SupportedFileExtensions { get; }

        /// <summary>
        /// Is this synchroniser available for the given <paramref name="solution"/> and <paramref name="project"/>.
        /// </summary>
        bool IsAvailable(Solution solution, Project project);

        /// <summary>
        /// Decides if the resource synchroniser can perform a synchronisation pass for the provided project file.
        /// <para/>
        /// Typically, this method should decide if a synchronisation pass makes sense for the file type in the current context.
        /// <para/>
        /// For example, if this synchroniser consumes Android .xml resoures but the project is an iOS project then the synchroniser should return false.
        /// </summary>
        Task<bool> CanSynchronise(Solution solution, Project project, IProjectFile projectFile);

        Task<bool> Synchronise(Solution solution, Project project, IProjectFile projectFile, ITextProvider textProvider, IProjectResourcesDatabase database);
    }
}
