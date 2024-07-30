using MFractor.Configuration;
using MFractor.Code.Documents;
using Microsoft.CodeAnalysis;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;
using CompilationProject = Microsoft.CodeAnalysis.Project;

namespace MFractor.Code
{
    public interface IFeatureContext : IMetaDataObject
    {
        CompilationWorkspace Workspace { get; }

        Solution Solution { get; }

        IParsedDocument Document { get; }

        CompilationProject Project { get; }

        object Syntax { get; set; }

        object SemanticModel { get; }

        ConfigurationId ConfigurationId { get; }

        TSyntax GetSyntax<TSyntax>(TSyntax defaultValue = null) where TSyntax : class;
    }
}
