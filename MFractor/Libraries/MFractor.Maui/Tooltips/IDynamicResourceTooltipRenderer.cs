using System;
using System.Threading.Tasks;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Tooltips
{
    public interface IDynamicResourceTooltipRenderer
    {
        string CreateTooltip(string dynamicResourceName, string currentXamlFile, Project project, IXamlPlatform platform);
        string CreateTooltip(string dynamicResourceName, Project project);
    }
}