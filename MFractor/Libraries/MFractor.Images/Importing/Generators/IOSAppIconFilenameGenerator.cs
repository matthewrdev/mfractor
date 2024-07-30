using System;
using System.Text;
using MFractor.Images.Models;

namespace MFractor.Images.Importing.Generators
{
    public class IOSAppIconFilenameGenerator : IAppIconFilenameGenerator
    {
        void Validate(IconImage image)
        {
            if (string.IsNullOrWhiteSpace(image.Name))
            {
                throw new ArgumentNullException(nameof(image.Name), "The property is required for composing the filename.");
            }
            if (image.Set is null || string.IsNullOrWhiteSpace(image.Set.Name))
            {
                throw new ArgumentNullException(nameof(image.Set), "The property is required for composing the filename.");
            }
        }

        public string Generate(IconImage image)
        {
            Validate(image);

            var builder = new StringBuilder();
            builder.Append(image.Name);
            builder.Append("-");
            builder.Append(image.Idiom.GetImportName());
            builder.Append("-");
            builder.Append(image.Set.ImportName);
            builder.AppendScale(image.Scale);
            builder.Append(image.FileExtension);
            return builder.ToString().ToLower();
        }
    }
}
