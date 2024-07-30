using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Code.CodeActions;
using Microsoft.CodeAnalysis;

namespace MFractor.Code
{
    [InheritedExport]
    public interface IFeatureContextFactory
    {
        IFeatureContext Retrieve(string fileName);

        void Store(IFeatureContext context, string fileName);

        CodeActionExecutionType[] SupportedExecutionTypes { get; }
    
        bool IsInterestedInDocument(Project project, string filePath);

        IFeatureContext CreateFeatureContext(Project project, string filePath, int interactionOffset);

        Task<IFeatureContext> CreateFeatureContextAsync(Project project, string filePath, int interactionOffset, CancellationToken token);

        IFeatureContext CreateFeatureContext(Project project, string filePath, object syntaxTree, int interactionOffset);

        Task<IFeatureContext> CreateFeatureContextAsync(Project project, string filePath, int interactionOffset, object syntaxTree, CancellationToken token);

        object GetSyntaxAtLocation(object syntaxTree, int offset);
    }
}
