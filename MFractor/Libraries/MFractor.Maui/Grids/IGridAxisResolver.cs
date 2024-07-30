using System;
using System.Collections.Generic;
using MFractor.Maui.XamlPlatforms;
using MFractor.Xml;

namespace MFractor.Maui.Grids
{
    public interface IGridAxisResolver
    {
        string GetDimensionNameForAxis(string axisName, IXamlPlatform platform);

        GridAxisDefinitionFormat GetRowAxisDefinitionFormat(XmlNode xmlNode, IXamlPlatform platform);

        GridAxisDefinitionFormat GetColumnAxisDefinitionFormat(XmlNode xmlNode, IXamlPlatform platform);

        GridAxisDefinitionFormat GetAxisDefinitionFormat(XmlNode xmlNode, string axisName);

        bool DeclaresRows(XmlNode xmlNode, IXamlPlatform platform);
        bool DeclaresColumns(XmlNode xmlNode, IXamlPlatform platform);

        IReadOnlyList<IGridAxisDefinition> ResolveRowDefinitions(XmlNode xmlNode, IXamlPlatform platform);
        IReadOnlyList<IGridAxisDefinition> ResolveRowDefinitions(XmlNode xmlNode, GridAxisDefinitionFormat axisDefinitionFormat, IXamlPlatform platform);

        IReadOnlyList<IGridAxisDefinition> ResolveColumnDefinitions(XmlNode xmlNode, IXamlPlatform platform);
        IReadOnlyList<IGridAxisDefinition> ResolveColumnDefinitions(XmlNode xmlNode, GridAxisDefinitionFormat axisDefinitionFormat, IXamlPlatform platform);

        IReadOnlyList<IGridAxisDefinition> ResolveDefinitions(XmlNode xmlNode, string axisName, IXamlPlatform platform);
        IReadOnlyList<IGridAxisDefinition> ResolveDefinitions(XmlNode xmlNode, string axisName, string dimensionName);
        IReadOnlyList<IGridAxisDefinition> ResolveDefinitions(XmlNode xmlNode, string axisName, GridAxisDefinitionFormat axisDefinitionFormat, IXamlPlatform platform);
        IReadOnlyList<IGridAxisDefinition> ResolveDefinitions(XmlNode xmlNode, string axisName, string dimensionName, GridAxisDefinitionFormat axisDefinitionFormat);

        IReadOnlyList<IGridAxisDefinition> ResolveNodeDefinedDefinitions(XmlNode xmlNode, string axisName, string dimensionName);
        IReadOnlyList<IGridAxisDefinition> ResolveAttributeDefinedDefinitions(XmlNode xmlNode, string axisName, string dimensionName);

    }
}