using System;
using System.Collections.Generic;
using MFractor.IOC;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.XamlPlatforms
{
    public interface IXamlPlatformRepository : IPartRepository<IXamlPlatform>
    {
        IReadOnlyList<IXamlPlatform> XamlPlatforms { get; }

        bool CanResolvePlatform(Project project);
        bool CanResolvePlatform(Project project, Compilation compilation);
        bool CanResolvePlatform(Project project, Compilation compilation, IXmlSyntaxTree xmlSyntaxTree);

        IXamlPlatform ResolvePlatform(Project project);
        IXamlPlatform ResolvePlatform(Project project, Compilation compilation);
        IXamlPlatform ResolvePlatform(Project project, Compilation compilation, IXmlSyntaxTree xmlSyntaxTree);
        IXamlPlatform ResolvePlatform(IXmlSyntaxTree xmlSyntaxTree);
    }
}