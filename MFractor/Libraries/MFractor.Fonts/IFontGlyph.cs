namespace MFractor.Fonts
{
    public interface IFontGlyph
    {
        string Name { get; }

        bool HasName { get; }

        string CharacterCodeHex { get; }

        string Unicode { get; }

        uint Codepoint { get; }

        bool IsCharacter { get; }

        bool IsIcon { get; }
    }
}
