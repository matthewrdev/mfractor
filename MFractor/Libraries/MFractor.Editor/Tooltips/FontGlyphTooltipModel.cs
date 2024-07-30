using MFractor.Fonts;

namespace MFractor.Editor.Tooltips
{
    public class FontGlyphTooltipModel
    {
        public FontGlyphTooltipModel(IFont font, string characterCode)
        {
            Font = font;
            CharacterCode = characterCode;
        }

        public IFont Font { get; }

        public string CharacterCode { get; }
    }
}
