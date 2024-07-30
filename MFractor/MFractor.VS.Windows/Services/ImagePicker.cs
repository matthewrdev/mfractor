using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFractor.Views;
using Microsoft.Win32;

namespace MFractor.VS.Windows.Services
{
    [Export(typeof(IImagePicker))]
    public class ImagePicker : IImagePicker
    {
        public Task<string> PickAsync(string dialogTitle)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = dialogTitle;
            openFileDialog.Filter = "Image files (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() ?? false)
            {
                return Task.FromResult(openFileDialog.FileName);
            }

            return Task.FromResult(default(string));
        }
    }
}
