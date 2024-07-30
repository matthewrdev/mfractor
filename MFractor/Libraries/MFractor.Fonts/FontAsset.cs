using Microsoft.CodeAnalysis;

namespace MFractor.Fonts
{
    class FontAsset : Font, IFontAsset
    {
        public Project Project { get; }

        public FontAsset(string filePath,
                         string name,
                         string fullName,
                         string style,
                         string postscriptName,
                         string typographicFamilyName,
                         bool isWebFont,
                         Project project) 
            : base(filePath, name, fullName, style, postscriptName, typographicFamilyName, isWebFont)
        {
            Project = project;
        }
    }
}
