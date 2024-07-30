using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFractor.Images;
using MFractor.Models;

namespace MFractor.VS.Windows.UI.ViewModels
{
    public class ImageImportPreviewViewModel : ObservableBase
    {
        public string ImagePath { get; }

        public string Destination { get; }

        public int Width { get; }

        public int Height { get; }

        public string SizeDescription => $"{Width}w by {Height}h";

        public ImageImportPreviewViewModel(ImageImportDescriptor imageDescriptor)
        {
            ImagePath = imageDescriptor.FilePath;
            Destination = imageDescriptor.VirtualPath;
            Width = imageDescriptor.ScaledWidth;
            Height = imageDescriptor.ScaledHeight;
        }
    }
}
