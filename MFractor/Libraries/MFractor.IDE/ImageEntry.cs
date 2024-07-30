using System;

namespace MFractor.Ide
{
    public class ImageEntry
    {
        public ImageEntry(string name, string filePath, Guid guid)
        {
            Name = name;
            FilePath = filePath;
            Guid = guid;
        }

        public string Name { get; }

        public string FilePath { get; }

        public Guid Guid { get; }
    }
}
