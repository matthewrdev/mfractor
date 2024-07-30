using MFractor.Code.CodeGeneration;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.iOS.Fonts
{
    public interface IFontPlistEntryGenerator : ICodeGenerator
    {
        IWorkUnit CreateIOSPlistEntry(Project project, string fontAssetName);

        string GenerateCode(string fontAssetName, FontPlistEntryKind plistEntryKind);
    }
}
