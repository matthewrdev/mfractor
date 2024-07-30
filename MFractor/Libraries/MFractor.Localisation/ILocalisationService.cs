using Microsoft.CodeAnalysis;

namespace MFractor.Localisation
{
    public interface ILocalisationService
    {
        bool CanLocalise(Project project, string filePath);
    }
}
