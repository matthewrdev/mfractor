using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using CompilationProject = Microsoft.CodeAnalysis.Project;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Code.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> inserts one or many syntax nodes a <see cref="HostNode"/>, using the <see cref="AnchorNode"/> and <see cref="InsertionLocation"/> to control the insertion position.
    /// </summary>
    public class InsertSyntaxNodesWorkUnit : WorkUnit
    {
        /// <summary>
        /// The workspace to insert the syntax into.
        /// </summary>
        /// <value>The workspace.</value>
        public CompilationWorkspace Workspace { get; set; }

        /// <summary>
        /// The project to insert the syntax into.
        /// </summary>
        /// <value>The project.</value>
        public CompilationProject Project { get; set; }

        /// <summary>
        /// The host syntax to insert the syntax into.
        /// </summary>
        /// <value>The host node.</value>
        public SyntaxNode HostNode { get; set; }

        /// <summary>
        /// The syntax node that the <see cref="SyntaxNodes"/> are inserted against.
        /// <para/>
        /// Use <see cref="InsertionLocation"/> to place the nodes before or after the anchor.
        /// </summary>
        public SyntaxNode AnchorNode { get; set; }

        /// <summary>
        /// The location to place the <see cref="SyntaxNodes"/> relative to the <see cref="AnchorNode"/>.
        /// <para/>
        /// If no <see cref="AnchorNode"/> is specified, the <see cref="SyntaxNodes"/> are placed at the start or end of the <see cref="HostNode"/>.
        /// </summary>
        public InsertionLocation InsertionLocation { get; set; } = InsertionLocation.Default;

        /// <summary>
        /// The syntax nodes to insert.
        /// </summary>
        /// <value>The syntax nodes.</value>
        public IReadOnlyList<SyntaxNode> SyntaxNodes { get; set; }
    }
}
