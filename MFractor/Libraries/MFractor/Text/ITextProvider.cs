using System;
using System.Threading.Tasks;

namespace MFractor.Text
{
    public interface ITextProvider
    {
        string GetText();

        Task<string> GetTextAsync();
    }
}
