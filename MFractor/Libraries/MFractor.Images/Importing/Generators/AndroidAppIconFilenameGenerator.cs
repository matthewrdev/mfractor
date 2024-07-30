using System;
using MFractor.Images.Models;

namespace MFractor.Images.Importing.Generators
{
    public class AndroidAppIconFilenameGenerator : IAppIconFilenameGenerator
    {
        public string Generate(IconImage image) => $"{image.Name.ToLower()}{image.FileExtension}";
    }
}
