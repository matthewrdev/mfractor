using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MFractor.Views
{
    /// <summary>
    /// Provides an abstraction of an Image Picker for platform independent selection of image file.
    /// </summary>
    interface IImagePicker
    {
        /// <summary>
        /// Pick an image file asynchronously returning its path when finished.
        /// </summary>
        /// <param name="title">The title to display on the dialog.</param>
        /// <returns></returns>
        Task<string> PickAsync(string dialogTitle);
    }
}
