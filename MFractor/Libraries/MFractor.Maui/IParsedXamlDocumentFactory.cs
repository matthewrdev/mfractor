using System.Threading.Tasks;
using MFractor.Code.Documents;
using MFractor.Text;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    public interface IParsedXamlDocumentFactory
    {
        IParsedXamlDocument Create(Project project, string filePath, ITextProvider textProvider);
        IParsedXamlDocument Create(Project project, IProjectFile projectFile, ITextProvider textProvider);

        Task<IParsedXamlDocument> CreateAsync(Project project, IProjectFile projectFile, ITextProvider textProvider);
    }
}