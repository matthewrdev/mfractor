using System;
using System.IO;
using MFractor.Text;

namespace MFractor.Text
{
    public interface ILineCollectionFactory
    {
        ILineCollection Create(string content);
        ILineCollection Create(Stream content);
        ILineCollection Create(ITextProvider textProvider);
        ILineCollection Create(FileInfo fileInfo);
    }
}