using System.Threading;
using System.Threading.Tasks;
using MFractor.Code;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    /// <summary>
    /// A <see cref="IFeatureContextFactory"/> that creates <see cref="IXamlFeatureContext"/>'s for XAML flavoured documents.
    /// </summary>
    public interface IXamlFeatureContextService : IFeatureContextFactory
    {
        IXamlFeatureContext CreateXamlFeatureContext(Project project, string filePath, int interactionOffset);

        Task<IXamlFeatureContext> CreateXamlFeatureContextAsync(Project project, string filePath, int interactionOffset, CancellationToken token);

        IXamlFeatureContext CreateXamlFeatureContext(Project project, string filePath, XmlSyntaxTree syntaxTree, int interactionOffset);

        Task<IXamlFeatureContext> CreateXamlFeatureContextAsync(Project project, string filePath, XmlSyntaxTree syntaxTree, int interactionOffset, CancellationToken token);
    }
}
