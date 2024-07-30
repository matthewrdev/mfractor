using System;
using System.Collections.Generic;
using System.Text;
using MFractor.Images.Models;

namespace MFractor.Images.Extensions
{
    /// <summary>
    /// Provides extension methods for the ImageScale type.
    /// </summary>
    public static class ImageScaleExtensions
    {
        public static ImageDensity ToDensity(this ImageScale scale) => 
            new ImageDensity(scale.Name, scale.Factor);
    }
}
