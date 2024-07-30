using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.XamlPlatforms
{
    [Export(typeof(IXamlPlatformRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class XamlPlatformRepository : PartRepository<IXamlPlatform>, IXamlPlatformRepository
    {
        [ImportingConstructor]
        public XamlPlatformRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IXamlPlatform> XamlPlatforms => Parts;

        public bool CanResolvePlatform(Project project)
        {
            return ResolvePlatform(project) != null;
        }

        public bool CanResolvePlatform(Project project, Compilation compilation)
        {
            return ResolvePlatform(project, compilation) != null;
        }

        public bool CanResolvePlatform(Project project, Compilation compilation, IXmlSyntaxTree xmlSyntaxTree)
        {
            return ResolvePlatform(project, compilation, xmlSyntaxTree) != null;
        }

        public IXamlPlatform ResolvePlatform(Project project)
        {
            if (project is null)
            {
                return null;
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            return ResolvePlatform(project, compilation);
        }

        public IXamlPlatform ResolvePlatform(Project project, Compilation compilation)
        {
            if (project is null || compilation is null)
            {
                return null;
            }

            var platform = XamlPlatforms.FirstOrDefault(p => p.IsSupported(project, compilation));

            return platform;
        }

        public IXamlPlatform ResolvePlatform(Project project, Compilation compilation, IXmlSyntaxTree xmlSyntaxTree)
        {
            if (project is null
                || compilation is null
                || xmlSyntaxTree is null)
            {
                return null;
            }

            var platform = XamlPlatforms.FirstOrDefault(p => p.IsSupported(project, compilation, xmlSyntaxTree));

            return platform;
        }

        public IXamlPlatform ResolvePlatform(IXmlSyntaxTree xmlSyntaxTree)
        {
            if (xmlSyntaxTree is null)
            {
                return null;
            }

            var platform = XamlPlatforms.FirstOrDefault(p => p.IsSupported(xmlSyntaxTree));

            return platform;
        }
    }
}