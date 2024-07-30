using System.Diagnostics;
using MFractor.Maui.Data.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    /// <summary>
    /// The result of a static resource search.
    /// </summary>
    [DebuggerDisplay("{Definition}")]
    public class DynamicResourceResult
    {
        /// <summary>
        /// The dynamic resource definition.
        /// </summary>
        /// <value>The definition.</value>
        public DynamicResourceDefinition Definition { get; }

        /// <summary>
        /// The project that owns the <see cref="Definition"/>/
        /// </summary>
        /// <value>The project.</value>
        public Project Project { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Maui.DynamicResourceResult"/> class.
        /// </summary>
        /// <param name="definition">Definition.</param>
        /// <param name="project">Project.</param>
        public DynamicResourceResult(DynamicResourceDefinition definition,
                                     Project project)
        {
            Definition = definition;
            Project = project;
        }
    }
}
