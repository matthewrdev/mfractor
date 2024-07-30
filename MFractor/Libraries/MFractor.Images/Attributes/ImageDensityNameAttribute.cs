using System;
using MFractor;

namespace MFractor.Images.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ImageDensityNameAttribute : Attribute
    {
        {
            Name = name;
            Platform = platform;
        }

        public string Name { get; }

        public PlatformFramework Platform { get; }
    }
}