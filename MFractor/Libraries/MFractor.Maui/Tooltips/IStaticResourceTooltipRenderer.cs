using System;
using MFractor.Maui.Data.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Tooltips
{
    public interface IStaticResourceTooltipRenderer
    {
        string CreateTooltip(StaticResourceDefinition definition, Project project, bool includeXmlPreview = true);
    }
}