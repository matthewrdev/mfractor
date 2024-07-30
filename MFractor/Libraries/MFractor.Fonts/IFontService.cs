using System.Threading.Tasks;

namespace MFractor.Fonts
{
    public interface IFontService
    {
        IFont GetFont(string fontFilePath);

        IFontTypeface GetFontTypeface(string fontFilePath);

        IFontTypeface GetFontTypeface(IFont font);

        bool IsFontFile(string fontFilePath);
    }
}
