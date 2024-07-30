using System;
using System.Collections.Generic;
using System.Drawing;
using MFractor.Code.CodeGeneration;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.Colors
{
    public interface IColorUsageConsolidator : ICodeGenerator
    {
        IReadOnlyList<IWorkUnit> Consolidate(Project project, IXamlPlatform platform, string resourceName, Color color, bool shouldCreateResourceDefinition);
    }
}