using System;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.Images.Data.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageAssetFile : ProjectFileOwnedEntity
    {
        public int ImageAssetDefinitionKey { get; set; }

        public UnifiedImageDensityKind Density { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string Extension { get; set; }

        public ImageAssetFileKind Kind { get; set; }
    }
}
