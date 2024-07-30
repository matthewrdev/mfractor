using System.IO;
using Microsoft.CodeAnalysis;

namespace MFractor.Fonts
{
    class Font : IFont
    {
        public string Name { get; }

        public string FullName { get; }

        public string Style { get; }

        public string PostscriptName { get; }

        public string FilePath { get; }

        public string FileName { get; }

        public string FamilyName { get; }

        public bool IsWebFont { get; }

        public Font(string filePath,
                    string name,
                    string fullName,
                    string style,
                    string postscriptName,
                    string typographicFamilyName,
                    bool isWebFont)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Name = name;
            FullName = fullName;
            Style = style;
            PostscriptName = postscriptName;
            FamilyName = typographicFamilyName;
            IsWebFont = isWebFont;
        }
    }
}