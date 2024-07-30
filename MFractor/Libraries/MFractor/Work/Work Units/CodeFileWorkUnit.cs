namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// The base class for a workUnit that creates or interacts with a code file.
    /// </summary>
    public abstract class CodeFileWorkUnit : WorkUnit
    {
        /// <summary>
        /// Gets or sets the file path for this code file workUnit.
        /// <para/>
        /// The file path can be either absolute or a relative/virtual path in a project.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the build action.
        /// </summary>
        /// <value>The build action.</value>
        public string BuildAction { get; set; }

        /// <summary>
        /// Gets a value indicating whether this workUnit has a build action.
        /// </summary>
        /// <value><c>true</c> if has build action; otherwise, <c>false</c>.</value>
        public bool HasBuildAction => !string.IsNullOrEmpty(BuildAction);

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        /// <value>The resource identifier.</value>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this workUnit has a resource identifier.
        /// </summary>
        /// <value><c>true</c> if has resource identifier; otherwise, <c>false</c>.</value>
        public bool HasResourceId => string.IsNullOrEmpty(ResourceId) == false;

        /// <summary>
        /// Gets or sets the depends upon file.
        /// </summary>
        /// <value>The depends upon file.</value>
        public string DependsUponFile { get; set; }

        /// <summary>
        /// Gets a value indicating whether this workUnit has a <see cref="DependsUponFile"/>.
        /// </summary>
        /// <value><c>true</c> if has depends upon file; otherwise, <c>false</c>.</value>
        public bool HasDependsUponFile => string.IsNullOrEmpty(DependsUponFile) == false;

        /// <summary>
        /// Gets or sets the custom tool namespace.
        /// </summary>
        /// <value>The custom tool namespace.</value>
        public string CustomToolNamespace { get; set; }

        /// <summary>
        /// Gets a value indicating whether this workUnit has a custom tool namespace.
        /// </summary>
        /// <value><c>true</c> if has custom tool namespace; otherwise, <c>false</c>.</value>
        public bool HasCustomToolNamespace => string.IsNullOrEmpty(CustomToolNamespace) == false;

        /// <summary>
        /// Gets or sets the generator.
        /// </summary>
        /// <value>The generator.</value>
        public string Generator { get; set; }

        /// <summary>
        /// Gets a value indicating whether this workUnit has a generator.
        /// </summary>
        /// <value><c>true</c> if has generator; otherwise, <c>false</c>.</value>
        public bool HasGenerator => string.IsNullOrEmpty(Generator) == false;
    }
}
