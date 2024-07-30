using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.StyleSheets
{
    /// <summary>
    /// Generates a new style sheet.
    /// </summary>
    public interface IStyleSheetGenerator : ICodeGenerator
    {
        /// <summary>
        /// The code snippet to use for the cascading style sheet.
        /// </summary>
        /// <value>The snippet.</value>
        ICodeSnippet Snippet { get; }

        /// <summary>
        /// The default folder to place new style sheets into.
        /// </summary>
        /// <value>The style folder.</value>
        string StyleFolder { get; }

        /// <summary>
        /// What is the default control when generating new style sheets?
        /// </summary>
        /// <value>The default control.</value>
        string DefaultControl { get; }

        /// <summary>
        /// Gets the file path for a style sheet.
        /// </summary>
        /// <returns>The file path for style sheet name.</returns>
        /// <param name="styleSheetName">Style sheet name.</param>
        string GetFilePathForStyleSheetName(string styleSheetName);

        /// <summary>
        /// Generate a new cascading style sheet for the controlName within the targetProject.
        /// <para/>
        /// This will ask for the name of the new style sheet and then generate the workUnits 
        /// </summary>
        /// <returns>A collection of workUnits that can be used to generate a new StyleSheet.</returns>
        /// <param name="project">The project to place the new style sheet into.</param>
        IReadOnlyList<IWorkUnit> Generate(Project project);

        /// <summary>
        /// Generate a new cascading style sheet using the provided controlName into the relativeFiletPath in  project.
        /// </summary>
        /// <returns>A collection of workUnits that generate the new StyleSheet.</returns>
        /// <param name="control">The unqualified name of the control.</param>
        /// <param name="relativeFilePath">The file path of the new cascading style sheet, relative to the project.</param>
        /// <param name="project">The project to place the new style sheet into.</param>
        IReadOnlyList<IWorkUnit> Generate(string control, string relativeFilePath, Project project);

        /// <summary>
        /// Generates the CSS for the provided control name.
        /// </summary>
        /// <returns>The code.</returns>
        /// <param name="control">The unqualified name of the control.</param>
        string Generate(string control);
    }
}
