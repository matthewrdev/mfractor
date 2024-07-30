using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.Thickness
{
    public interface IThicknessUsageConsolidator : ICodeGenerator
    {
        IReadOnlyList<IWorkUnit> Consolidate(Project project, IXamlPlatform platform, string resourceName, IReadOnlyList<double> thickness, bool shouldCreateResourceDefinition);
        IReadOnlyList<IWorkUnit> Consolidate(Project project, IXamlPlatform platform, string resourceName, double left, double right, double top, double bottom, bool shouldCreateResourceDefinition);
        IReadOnlyList<IWorkUnit> Consolidate(Project project, IXamlPlatform platform, string resourceName, string thicknessFormattedValue, bool shouldCreateResourceDefinition);
    }
}