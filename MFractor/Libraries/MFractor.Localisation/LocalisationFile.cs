
using System.IO;

namespace MFractor.Localisation
{
    public class LocalisationFile : ILocalisationFile
    {
        public string DisplayPath { get; }

        public string FullPath { get; }

        public string Extension => Path.GetExtension(FullPath);

        public LocalisationFile(string displayPath,
                                string fullPath)
        {
            DisplayPath = displayPath;
            FullPath = fullPath;
        }

        public LocalisationFile(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new System.ArgumentNullException(nameof(fileInfo));
            }

            DisplayPath = fileInfo.Name;
            FullPath = fileInfo.FullName;
        }

        public override string ToString()
        {
            return DisplayPath;
        }
    }
}
