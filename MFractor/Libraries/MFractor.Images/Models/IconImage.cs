using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using MFractor.Code.Analysis.SolutionAnalysis;
using MFractor.Attributes;
using MFractor.Images.Importing.Generators;
using MFractor.Models;

namespace MFractor.Images.Models
{
    /// <summary>
    /// Represents an App Icon Image.
    /// </summary>
    public class IconImage
    {
        /// <summary>
        /// The name for the image.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The idiom on the device to which this icon is targeted.
        /// </summary>
        public IconIdiom Idiom { get; set; }

        /// <summary>
        /// The icon set in which this icon is contained.
        /// </summary>
        public IconSet Set { get; set; }

        /// <summary>
        /// The scale of the image.
        /// </summary>
        public ImageScale Scale { get; set; }

        /// <summary>
        /// Is this image an Android adaptive icon image?
        /// </summary>
        public bool IsAdaptive { get; }

        /// <summary>
        /// A string describing the size in the format of WxH in the base unit.
        /// </summary>
        public string SizeDescription => $"{Set.Size}x{Set.Size}";

        /// <summary>
        /// The Width of the image in pixels.
        /// </summary>
        public int PixelWidth => (int)(Set.Size * Scale.Factor);

        /// <summary>
        /// The Height of the image in pixels.
        /// </summary>
        public int PixelHeight => (int)(Set.Size * Scale.Factor);

        /// <summary>
        /// Gets the destination folder for the image.
        /// </summary>
        public string DestinationFolder => GetDestinationFolder();

        /// <summary>
        /// Gets the file name for this image.
        /// </summary>
        public string FileName => BuildFileName();

        /// <summary>
        /// Gets a flag indicating if the file should be visible on the project.
        /// </summary>
        public bool IsProjectFileVisible => !Scale.IsAppleScale;

        /// <summary>
        /// Gets the extension of the 
        /// </summary>
        public string FileExtension => ".png"; // Always output in png

        public IconImage(IconIdiom idiom, IconSet set, ImageScale scale, bool isAdaptive = false)
        {
            Scale = scale;
            IsAdaptive = isAdaptive;
            Set = set;
            Idiom = idiom;
            Name = GetBaseFileName();
        }

        string GetBaseFileName() => Scale.IsAndroidScale ? ( IsAdaptive ? "launcher_foreground" : "icon")  : "AppIcon";

        string GetDestinationFolder() =>
            Scale.IsAndroidScale
            ? Path.Combine("Resources", $"mipmap-{Scale.Name}")
            : Path.Combine("Assets.xcassets", "AppIcon.appiconset");

        string BuildFileName()
        {
            var generator = GetFilenameGenerator();
            return generator.Generate(this);
        }

        IAppIconFilenameGenerator GetFilenameGenerator()
        {
            if (Set.Unit == ScaleUnit.DP)
            {
                return new AndroidAppIconFilenameGenerator();
            }
            if (Set == IconSet.CarPlay)
            {
                return new CarPlayAppIconFilenameGenerator();
            }
            return new IOSAppIconFilenameGenerator();
        }

        /// <summary>
        /// A helper method for executing a method inside a try...catch block with a default value when a exception is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the value being returned by the function block.</typeparam>
        /// <param name="defaultValue"></param>
        /// <param name="tryBlock"></param>
        /// <returns></returns>
        static T TryOrDefault<T>(T defaultValue, Func<T> tryBlock)
        {
            try
            {
                return tryBlock();
            }
            catch (Exception)
            {
                // TODO: Add some logging
                return defaultValue;
            }
        }
    }
}
