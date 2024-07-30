using MFractor.Code;
using MFractor.Configuration;
using MFractor.Maui.Semantics;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Maui
{
    class XamlFeatureContext : FeatureContext, IXamlFeatureContext
    {
        public Compilation Compilation { get; }

        public IXamlPlatform Platform { get; }

        public IXamlSemanticModel XamlSemanticModel => SemanticModel as IXamlSemanticModel;

        public IXamlNamespaceCollection Namespaces => XamlDocument.Namespaces;

        public IXmlnsDefinitionCollection XmlnsDefinitions => XamlDocument.XmlnsDefinitions;

        public IParsedXamlDocument XamlDocument => Document as IParsedXamlDocument;

        public XmlSyntaxTree SyntaxTree => XamlDocument.GetSyntaxTree();

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Maui.XamlFeatureContext"/> class.
        /// </summary>
        /// <param name="workspace">Workspace.</param>
        /// <param name="solution">Solution.</param>
        /// <param name="project">Project.</param>
        /// <param name="document">Document.</param>
        /// <param name="compilation">The compilation.</param>
        /// <param name="syntax">Syntax.</param>
        /// <param name="configuration">Configuration.</param>
        public XamlFeatureContext(CompilationWorkspace workspace,
                                  Solution solution,
                                  Project project,
                                  IParsedXamlDocument document,
                                  Compilation compilation,
                                  IXamlSemanticModel semanticModel,
                                  IXamlPlatform xamlPlatform,
                                  object syntax,
                                  ConfigurationId configuration)
            : base(workspace, solution, project, document, syntax, semanticModel, configuration)
        {
            Compilation = compilation;
            Platform = xamlPlatform;
        }
    }
}
