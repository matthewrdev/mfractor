using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Maui.CodeGeneration.Mvvm
{
    /// <summary>
    /// The <see cref="IViewViewModelGenerator"/> generates a XAML view, code behind and view model.
    /// <para/>
    /// Use the <see cref="IViewViewModelGenerator"/> to simplify the creation of these three dependant files.
    /// </summary>
    public interface IViewViewModelGenerator : ICodeGenerator
    {
        /// <summary>
        /// What is the default suffix to apply to a new view?
        /// </summary>
        /// <value>The view suffix.</value>
        string ViewSuffix { get; set; }

        /// <summary>
        /// Where should new views be placed?
        /// </summary>
        /// <value>The views folder.</value>
        string ViewsFolder { get; set; }

        /// <summary>
        /// When the <see cref="ViewBaseClass"/> is not a platform class, what is the XMLNS prefix to use?
        /// </summary>
        /// <value>The default name of the view xmlns.</value>
        string ViewXmlnsPrefix { get; set; }

        /// <summary>
        /// If no folder path is provided to the MVVM wizard, what is the default location the MVVM wizard should use?
        /// <para/>
        /// This configuration supports the use of the $name$ code snippet argument.
        /// </summary>
        /// <value>The default folder.</value>
        string DefaultFolderLocation { get; set; }

        /// <summary>
        /// What is the default base name to provide to the MVVM wizard?
        /// </summary>
        /// <value>The default name of the base.</value>
        string DefaultBaseName { get; set; }

        /// <summary>
        /// What is the id of the binding context connector that should be used to connect the view model to the XAML view?
        /// </summary>
        /// <value>The id of the default binding context connector..</value>
        string DefaultBindingContextConnectorId { get; set; }

        IReadOnlyList<IWorkUnit> Generate(Project project, IXamlPlatform platform, ViewViewModelGenerationOptions options);

        IReadOnlyList<IWorkUnit> Generate(ProjectIdentifier projectIdentifier, CompilationWorkspace workspace, IXamlPlatform platform, ViewViewModelGenerationOptions options);

        IReadOnlyList<IWorkUnit> Generate(Project project, IXamlPlatform platform, string virtualFolderPath, string baseName);

        IReadOnlyList<IWorkUnit> Generate(ProjectIdentifier projectIdentifier, CompilationWorkspace workspace, IXamlPlatform platform, string virtualFolderPath, string baseName);
    }
}
