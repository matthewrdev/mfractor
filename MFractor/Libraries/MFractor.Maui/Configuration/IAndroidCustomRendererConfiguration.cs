using System;
using MFractor.CodeSnippets;
using MFractor.Configuration;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Configuration
{
    /// <summary>
    /// A configuration for generating custom renderers for Android projects.
    /// </summary>
    public interface IAndroidCustomRendererConfiguration : IConfigurable
    {
        /// <summary>
        /// The code snippet for page custom renderers.
        /// </summary>
        /// <value>The page renderer snippet.</value>
        ICodeSnippet PageRendererSnippet { get; set; }

        /// <summary>
        /// The code snippet for layout custom renderers.
        /// </summary>
        /// <value>The layout renderer snippet.</value>
        ICodeSnippet LayoutRendererSnippet { get; set; }

        /// <summary>
        /// The code snippet for view custom renderers.
        /// </summary>
        /// <value>The view renderer snippet.</value>
        ICodeSnippet ViewRendererSnippet { get; set; }

        /// <summary>
        /// The code snippet for cell custom renderers.
        /// </summary>
        /// <value>The cell renderer snippet.</value>
        ICodeSnippet CellRendererSnippet { get; set; }
    }
}
