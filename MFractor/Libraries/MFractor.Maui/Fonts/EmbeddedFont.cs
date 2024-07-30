using System;
using System.IO;
using MFractor.Fonts;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Fonts
{
    class EmbeddedFont : IEmbeddedFont
    {
        readonly Lazy<IFont> font;

        public EmbeddedFont(string fontFileName,
                            string alias,
                            Project project,
                            Lazy<IFont> font)
        {
            if (string.IsNullOrEmpty(fontFileName))
            {
                throw new ArgumentException("message", nameof(fontFileName));
            }

            FontFileName = fontFileName;
            Alias = alias ?? string.Empty;
            CompilationProject = project;
            this.font = font;
        }

        public string FontFileName { get; }

        public string Alias { get; }

        public Project CompilationProject { get; }

        public IFont Font => font.Value;

        public string FontName => Path.GetFileNameWithoutExtension(FontFileName);

        public string FontFileExtension => Path.GetExtension(FontFileName);

        public bool HasAlias => !string.IsNullOrEmpty(Alias);

        public string LookupName => HasAlias ? Alias : FontName;
    }
}