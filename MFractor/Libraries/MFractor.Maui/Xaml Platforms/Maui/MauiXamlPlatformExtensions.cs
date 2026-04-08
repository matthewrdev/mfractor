using Microsoft.CodeAnalysis;
using MFractor.Xml;

namespace MFractor.Maui.XamlPlatforms.Maui
{
    public static class MauiXamlPlatformExtensions
    {
        public static bool Supports(this MauiXamlPlatform platform, Project project)
        {
            if (platform is null || project is null || !project.TryGetCompilation(out var compilation))
            {
                return false;
            }

            return platform.IsSupported(project, compilation);
        }

        public static IXamlPlatform Resolve(this MauiXamlPlatform platform, Project project)
        {
            if (platform.Supports(project))
            {
                return platform;
            }

            return null;
        }

        public static IXamlPlatform Resolve(this MauiXamlPlatform platform, Project project, Compilation compilation)
        {
            if (platform is null || project is null || compilation is null)
            {
                return null;
            }

            return platform.IsSupported(project, compilation) ? platform : null;
        }

        public static IXamlPlatform Resolve(this MauiXamlPlatform platform, Project project, Compilation compilation, IXmlSyntaxTree xmlSyntaxTree)
        {
            if (platform is null || project is null || compilation is null || xmlSyntaxTree is null)
            {
                return null;
            }

            return platform.IsSupported(project, compilation, xmlSyntaxTree) ? platform : null;
        }

        public static IXamlPlatform Resolve(this MauiXamlPlatform platform, IXmlSyntaxTree xmlSyntaxTree)
        {
            if (platform is null || xmlSyntaxTree is null)
            {
                return null;
            }

            return platform.IsSupported(xmlSyntaxTree) ? platform : null;
        }
    }
}
