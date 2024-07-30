using System;
using MFractor;

namespace MFractor.Images.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ImageDensityNameAttribute : Attribute
    {        public ImageDensityNameAttribute(string name, PlatformFramework platform)
        {
            Name = name;
            Platform = platform;
        }

        public string Name { get; }

        public PlatformFramework Platform { get; }
    }
}
