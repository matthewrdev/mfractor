using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MFractor.Workspace;

namespace MFractor.Fonts.Utilities
{
    public static class FontAssetHelper
    {
        public static readonly IReadOnlyList<string> FontAssetExtensions = new List<string>()
        {
            ".otf",
            ".ttf",
            ".woff",
        };

        public static bool IsFontAsset(this IProjectFile projectFile)
        {
            if (projectFile is null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            return FontAssetExtensions.Contains(projectFile.Extension);
        }

        public static bool IsFontAsset(this string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            return FontAssetExtensions.Contains(Path.GetExtension(filePath));
        }
    }
}
