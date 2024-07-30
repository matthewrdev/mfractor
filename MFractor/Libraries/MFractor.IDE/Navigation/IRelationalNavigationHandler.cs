using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Configuration;
using MFractor.Work;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.Navigation
{
    [InheritedExport]
    public interface IRelationalNavigationHandler : IConfigurable
    {
        bool IsAvailable(Project project, string filePath);

        RelationalNavigationContextType ResolveRelationalNavigationContextType(IProjectFile projectFile);

        string DefinitionDisplayName { get; }

        string DefinitionCodeBehindDisplayName { get; }

        string ImplementationDisplayName { get; }

        bool CanNavigateToImplementation(Project project, string filePath, ConfigurationId configId);
        IReadOnlyList<IWorkUnit> NavigateToImplementation(Project project, string filePath, ConfigurationId configId);

        bool CanNavigateToDefinition(Project project, string filePath, ConfigurationId configId);
        IReadOnlyList<IWorkUnit> NavigateToDefinition(Project project, string filePath, ConfigurationId configId);

        bool CanNavigateToDefinitionCodeBehind(Project project, string filePath, ConfigurationId configId);
        IReadOnlyList<IWorkUnit> NavigateToDefinitionCodeBehind(Project project, string filePath, ConfigurationId configId);
    }
}
