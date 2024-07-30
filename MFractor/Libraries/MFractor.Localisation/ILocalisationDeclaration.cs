using MFractor.Workspace;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation
{
    public interface ILocalisationDeclaration
    {
        IProjectFile ProjectFile { get; }

        string Key { get; }

        string Value { get; }

        string CultureCode { get; }

        TextSpan KeySpan { get; }

        TextSpan ValueSpan { get; }
    }
}
