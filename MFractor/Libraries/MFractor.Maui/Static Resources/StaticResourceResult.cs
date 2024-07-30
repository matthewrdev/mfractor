using System.Diagnostics;
using MFractor.Maui.Data.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.StaticResources
{
    /// <summary>
    /// The result of a static resource search.
    /// </summary>
    [DebuggerDisplay("{Definition}")]
    public class StaticResourceResult
    {
        /// <summary>
        /// The static resource definition.
        /// </summary>
        /// <value>The definition.</value>
        public StaticResourceDefinition Definition { get; }

        /// <summary>
        /// The project that owns the <see cref="Definition"/>/
        /// </summary>
        /// <value>The project.</value>
        public Project Project { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Maui.StaticResourceResult"/> class.
        /// </summary>
        /// <param name="definition">Definition.</param>
        /// <param name="project">Project.</param>
        public StaticResourceResult(StaticResourceDefinition definition,
                                    Project project)
        {
            Definition = definition;
            Project = project;
        }
    }
}
