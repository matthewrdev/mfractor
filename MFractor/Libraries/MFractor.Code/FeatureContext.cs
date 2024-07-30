using System.Linq;
using MFractor.Configuration;
using MFractor.Code.Documents;
using Microsoft.CodeAnalysis;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;
using CompilationProject = Microsoft.CodeAnalysis.Project;
using CompilationSolution = Microsoft.CodeAnalysis.Solution;

namespace MFractor.Code
{
    /// <summary>
    /// A feature context describes the environment that a feature is interacting with.
    /// </summary>
    public class FeatureContext : MetaDataObject, IFeatureContext
    {
        /// <summary>
        /// The current workspace.
        /// </summary>
        /// <value>The workspace.</value>
        public CompilationWorkspace Workspace { get; }

        /// <summary>
        /// The current solution.
        /// </summary>
        /// <value>The solution.</value>
        public CompilationSolution Solution { get; }

        /// <summary>
        /// The current project.
        /// </summary>
        public CompilationProject Project { get; }

        /// <summary>
        /// The current document.
        /// </summary>
        /// <value>The document.</value>
        public IParsedDocument Document { get; }

        /// <summary>
        /// The current syntax element.
        /// </summary>
        /// <value>The syntax.</value>
        public object Syntax { get; set; }

        /// <summary>
        /// The current semantic model.
        /// </summary>
        /// <value>The syntax.</value>
        public object SemanticModel { get; internal set; }

        /// <summary>
        /// The configuration identifier for the current feature context.
        /// <para/>
        /// This can be used to apply the correct user configuration onto IConfigurables.
        /// </summary>
        /// <value>The configuration identifier.</value>
        public ConfigurationId ConfigurationId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.FeatureContext"/> class.
        /// </summary>
        /// <param name="workspace">Workspace.</param>
        /// <param name="solution">Solution.</param>
        /// <param name="project">Project.</param>
        /// <param name="document">Document.</param>
        /// <param name="syntax">Syntax.</param>
        /// <param name="configuration">Configuration.</param>
        public FeatureContext(CompilationWorkspace workspace,
                              CompilationSolution solution,
                              CompilationProject project,
                              IParsedDocument document,
                              object syntax,
                              object semanticModel,
                              ConfigurationId configuration)
        {
            ConfigurationId = configuration;
            Document = document;
            Syntax = syntax;
            SemanticModel = semanticModel;
            Workspace = workspace;
            Solution = solution;
            Project = project;
        }

        /// <summary>
        /// Gets the syntax, cast as <typeparamref name="TSyntax"/>.
        /// </summary>
        /// <returns>The syntax.</returns>
        /// <param name="defaultValue">Default value.</param>
        /// <typeparam name="TSyntax">The syntax type to cast to.</typeparam>
        public TSyntax GetSyntax<TSyntax>(TSyntax defaultValue = default) where TSyntax : class
        {
            return Syntax as TSyntax ?? defaultValue;
        }
    }
}
