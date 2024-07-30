using System;
using System.Collections.Generic;

namespace MFractor.Ide
{
    /// <summary>
    /// Registers images into the IDEs image managament system for display via tooltips.
    /// </summary>
    public interface IIdeImageManager
    {
        Guid ImageTooltipId { get; }

        void SetImageTooltipFile(string filePath);

        bool HasImage(string name);

        bool HasImage(Guid guid);

        void AddImage(string name, string filePath, Guid guid);

        void AddImage(ImageEntry image);

        void AddImages(IReadOnlyList<ImageEntry> icons);

        Guid GetGuid(string name);

        string GetImage(Guid guid);
    }
}
