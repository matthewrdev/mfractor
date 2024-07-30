using System;
using System.Diagnostics;

namespace MFractor.Fonts
{
    [DebuggerDisplay("{Name} - {CharacterCode} - {UnicodeValue}")]
    class FontGlyph : IFontGlyph
    {
        public string Name { get; }

        public bool HasName => !string.IsNullOrEmpty(Name);

        public string CharacterCodeHex { get; }

        public uint Codepoint { get; }

        public bool IsCharacter => !IsIcon;

        // If the lenght is 4, this is a unicode icon string rather than an ascii code. EG: f001.
        public bool IsIcon => CharacterCodeHex.Length == 4;

        public string Unicode { get; }

        public FontGlyph(string name, string code, uint codepoint)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("message", nameof(name));
            }

            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("message", nameof(code));
            }

            Name = name;
            CharacterCodeHex = code;
            Codepoint = codepoint;
            Unicode = char.ToString((char)Codepoint);
        }
    }
}