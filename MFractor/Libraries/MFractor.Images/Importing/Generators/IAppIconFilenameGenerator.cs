using System;
using MFractor.Images.Models;

namespace MFractor.Images.Importing.Generators
{
    public interface IAppIconFilenameGenerator
    {
        string Generate(IconImage image);
    }
}
