using System;

namespace MFractor.Maui.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TargetXamlPlatformAttribute : Attribute
    {
        public TargetXamlPlatformAttribute(XamlPlatform xamlPlatform)
        {
            XamlPlatform = xamlPlatform;
        }

        /// <summary>
        /// The XAML platform that this feature supports.
        /// </summary>
        public XamlPlatform XamlPlatform { get; }
    }
}
