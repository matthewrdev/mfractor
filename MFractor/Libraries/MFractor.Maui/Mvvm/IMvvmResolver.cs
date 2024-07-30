using MFractor.Ide.Navigation;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Mvvm
{
    /// <summary>
    /// The MVVM Resolver intelligently locates the View, Code Behind or View Model for a given file within a project.
    /// </summary>
    public interface IMvvmResolver
    {
        /// <summary>
        /// The name of the design time binding context attribute.
        /// <para/>
        /// This attribute can be added to the code behind class declaration to direct MFractor towards the intended binding context for that class.
        /// </summary>
        /// <value>The type of the design binding context attribute meta.</value>
        string DesignTimeBindingContextAttributeName { get; }

        /// <summary>
        /// The supported suffixes that indicate a class is a view model.
        /// </summary>
        /// <value>The view model suffixes.</value>
        string[] ViewModelSuffixes { get; }

        /// <summary>
        /// The supported suffixes that indicate a class is a XAML view.
        /// </summary>
        /// <value>The view suffixes.</value>
        string[] ViewSuffixes { get; }

        /// <summary>
        /// Resolves the design time binding context for the provided <paramref name="project"/> and <paramref name="filePath"/>.
        /// </summary>
        /// <returns>The design time binding context async.</returns>
        /// <param name="project">Project.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="configId">Config identifier.</param>
        INamedTypeSymbol ResolveDesignTimeBindingContext(Project project, string filePath);

        INamedTypeSymbol ResolveDesignTimeBindingContext(Compilation compilation, INamedTypeSymbol codeBehind);

        /// <summary>
        /// Resolves the <see cref="RelationalNavigationContextType"/> for the given file path.
        /// </summary>
        RelationalNavigationContextType ResolveContextType(string filePath);

        /// <summary>
        /// For a given file in a project, resolves the corresponding view.
        /// </summary>
        /// <returns>The xaml view for context async.</returns>
        /// <param name="project">Project.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="configId">Config identifier.</param>
        string ResolveXamlView(Project project, string filePath, bool considerProjectReferences = true);

        /// <summary>
        /// Resolve the ViewModel.
        /// </summary>
        /// <returns>The view model symbol async.</returns>
        /// <param name="project">Project.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="configId">Config identifier.</param>
        INamedTypeSymbol ResolveViewModelSymbol(Project project, string filePath, bool considerProjectReferences = true);
        
        INamedTypeSymbol ResolveViewModelSymbol(Project project,
                                                string filePath,
                                                XmlSyntaxTree syntaxTree,
                                                Compilation compilation,
                                                IXamlPlatform xamlPlatform,
                                                IXamlNamespaceCollection namespaces,
                                                IXmlnsDefinitionCollection xmlnsDefinitions,
                                                bool considerProjectReferences = true);

        INamedTypeSymbol ResolveCodeBehindSymbol(Project project, string filePath, bool considerProjectReferences = true);
    }
}
