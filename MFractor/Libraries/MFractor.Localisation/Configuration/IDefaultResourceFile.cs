using System;
using MFractor.Configuration;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.Configuration
{
    /// <summary>
    /// A configuration object that groups the configuration settings for the default resource file for a users project.
    /// </summary>
    public interface IDefaultResourceFile : IConfigurable
    {
        /// <summary>
        /// The file path of the default resource file relative to the project root.
        /// 
        /// For example:
        /// 
        /// Resources/AppResources.resx
        /// </summary>
        /// <value>The project file path.</value>
        string ProjectFilePath { get; set; }

        /// <summary>
        /// Gets the name of the fully qualified default resource symbol using the provided feature context.
        /// </summary>
        /// <returns>The fully qualified default resource symbol name.</returns>
        /// <param name="project">The project to get the resource suymbol name.</param>
        string GetFullyQualifiedDefaultResourceSymbolName(Project project);
    }
}
