using System;
using System.Text;
using MFractor.Images.Models;

namespace MFractor.Images.Importing.Generators
{
    static class AppIconFilenameGeneratorExtensions
    {
        public static void AppendScale(this StringBuilder builder, ImageScale scale)
        {
            if (scale.IsAppleScale && scale != ImageScale.Points_1x)
            {
                builder.Append("@");
                builder.Append(scale.Name);
            }
        }
    }
}
