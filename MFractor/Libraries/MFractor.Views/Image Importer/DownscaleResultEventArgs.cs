using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Images;

namespace MFractor.Views.ImageImporter
{
    public class ImportImageEventArgs : EventArgs
    {
        public string ImageFilePath { get; }
        public string ImageName { get; }

        public ImportImageEventArgs(string imageFilePath,
                                        string imageName)
        {
            ImageFilePath = imageFilePath;
            ImageName = imageName;
        }
    }
}
