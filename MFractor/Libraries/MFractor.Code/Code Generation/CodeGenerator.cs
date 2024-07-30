using System;
using System.ComponentModel.Composition;
using MFractor.Code.Formatting;
using MFractor.Configuration;
using MFractor.Workspace;
using MFractor.Xml;

namespace MFractor.Code.CodeGeneration
{
    /// <summary>
    /// The base class for all MFractor code generators.
    /// </summary>
    public abstract class CodeGenerator : Configurable, ICodeGenerator
    {
        [Import]
#pragma warning disable IDE1006 // Naming Styles
        Lazy<ICodeFormattingPolicyService> formattingPolicyService { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [Import]
#pragma warning disable IDE1006 // Naming Styles
        Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [Import]
#pragma warning disable IDE1006 // Naming Styles
        Lazy<IProjectService> projectService { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets the languages that this code generator supports.
        /// </summary>
        /// <value>The languages.</value>
        public abstract string[] Languages { get; }

        /// <summary>
        /// Gets the code formatting policy service.
        /// </summary>
        /// <value>The formatting policy service.</value>
        protected ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        /// <summary>
        /// Gets the formatting policy service.
        /// </summary>
        /// <value>The formatting policy service.</value>
        protected IXmlFormattingPolicyService XmlFormattingPolicyService => xmlFormattingPolicyService.Value;

        /// <summary>
        /// Gets the project information service.
        /// </summary>
        /// <value>The project information service.</value>
        protected IProjectService ProjectService => projectService.Value;
    }
}
