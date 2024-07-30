using System;
using System.IO;
using MFractor.IOC;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that creates a new file in the <see cref="CodeFileWorkUnit.TargetProject"/>. 
    /// </summary>
    public class CreateProjectFileWorkUnit : CodeFileWorkUnit
    {
        public CreateProjectFileWorkUnit()
        {

        }

        public CreateProjectFileWorkUnit(string fileContent, string filePath, ProjectIdentifier projectIdentifier)
        {
            FileContent = fileContent;
            FilePath = filePath;
            TargetProjectIdentifier = projectIdentifier;
        }

        public CreateProjectFileWorkUnit(string fileContent, string filePath, Project project)
        {
            FileContent = fileContent;
            FilePath = filePath;
            TargetProject = project;
        }

        /// <summary>
        /// Is the 
        /// </summary>
        /// <value><c>true</c> if is binary; otherwise, <c>false</c>.</value>
        public bool IsBinary { get; set; } = false;

        public Action<Stream> WriteContentAction { get; set; }

        /// <summary>
        /// Gets or sets the Virtual File Path (the path of the file inside the project structure).
        /// </summary>
        public string VirtualFilePath { get; set; }

        /// <summary>
        /// The content of the new file.
        /// </summary>
        /// <value>The content of the file.</value>
        public string FileContent { get; set; }

        /// <summary>
        /// The identifier for the target project.
        /// <para/>
        /// Using project identifiers allows MFractor to generate into shared assets project and circumvent Roslyns project model restrictions.
        /// </summary>
        /// <value>The target project identifier.</value>
        public ProjectIdentifier TargetProjectIdentifier { get; set; }

        /// <summary>
        /// The project that the new file should be created in.
        /// </summary>
        /// <value>The target project.</value>
        public Project TargetProject
        {
            get => Resolver.Resolve<IProjectService>().GetProject(TargetProjectIdentifier);
            set => TargetProjectIdentifier = value == null ? null : value.GetIdentifier();
        }

        /// <summary>
        /// Should the IDE try to infer if the new file is being placed into a shared project and if so, place the file in their instead of the <see cref="TargetProject"/> .
        /// </summary>
        /// <value><c>true</c> if infer when in shared project; otherwise, <c>false</c>.</value>
        public bool InferWhenInSharedProject { get; set; } = true;

        /// <summary>
        /// When the project file is generated, should the work engine allow post-processers (such as syntax reducers) to run against the new files content?
        /// </summary>
        public bool AllowPostProcessing { get; set; } = true;

        /// <summary>
        /// After create the new file, should MFractor open the file inside the IDE?
        /// </summary>
        /// <value><c>true</c> if should open; otherwise, <c>false</c>.</value>
        public bool ShouldOpen { get; set; } = true;

        /// <summary>
        /// If the new file already exists, should it be overwritten?
        /// </summary>
        /// <value><c>true</c> if should over write; otherwise, <c>false</c>.</value>
        public bool ShouldOverWrite { get; set; } = false;

        /// <summary>
        /// The project file should be visible to the project. This is used to add
        /// entries that must be part of the MSBuild but shouldn't be visible from
        /// the IDE. The effect is to a Visible element to the file declaration on
        /// the project file.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Should the folders of the project be added to the Project file as
        /// '<Folder Include="">' entries.
        /// </summary>
        public bool ShouldAddFoldersToMsBuild { get; set; } = true;
    }
}
