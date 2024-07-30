using System;
using System.Collections.Generic;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    public interface IXmlnsDefinitionResolver
    {
        IXmlnsDefinitionCollection Resolve(Project project, IXamlPlatform platform);
    }
}