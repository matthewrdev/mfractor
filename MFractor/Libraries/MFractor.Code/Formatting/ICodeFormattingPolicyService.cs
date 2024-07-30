using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.Formatting
{
    /// <summary>
    /// The formatting policy service can be used to get the C# formating policies for a project.
    /// </summary>
    public interface ICodeFormattingPolicyService
    {
        /// <summary>
        /// Get the default C# formatting options.
        /// </summary>
        /// <returns>The formatting options.</returns>
        ICSharpFormattingPolicy GetFormattingPolicy();

        /// <summary>
        /// Get the formatting options for the provided feature context.
        /// </summary>
        /// <returns>The formatting options.</returns>
        /// <param name="context">Context.</param>
        ICSharpFormattingPolicy GetFormattingPolicy(IFeatureContext context);

        /// <summary>
        /// Get the C# formatting options for the provided project.
        /// </summary>
        /// <returns>The formatting options.</returns>
        /// <param name="project">Project.</param>
        ICSharpFormattingPolicy GetFormattingPolicy(Project project);

        /// <summary>
        /// Get the C# formatting options for the provided project.
        /// </summary>
        /// <returns>The formatting options.</returns>
        /// <param name="projectIdentifier">Project.</param>
        ICSharpFormattingPolicy GetFormattingPolicy(ProjectIdentifier projectIdentifier);
    }
}
