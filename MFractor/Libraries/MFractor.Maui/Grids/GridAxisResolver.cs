using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Grids
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IGridAxisResolver))]
    class GridAxisResolver : IGridAxisResolver
    {
        public bool DeclaresRows(XmlNode xmlNode, IXamlPlatform platform)
        {
            return GetRowAxisDefinitionFormat(xmlNode, platform) != GridAxisDefinitionFormat.Undefined;
        }

        public bool DeclaresColumns(XmlNode xmlNode, IXamlPlatform platform)
        {
            return GetColumnAxisDefinitionFormat(xmlNode, platform) != GridAxisDefinitionFormat.Undefined;
        }

        public GridAxisDefinitionFormat GetColumnAxisDefinitionFormat(XmlNode xmlNode, IXamlPlatform platform)
        {
            return GetAxisDefinitionFormat(xmlNode, platform.ColumnDefinitionsProperty);
        }

        public GridAxisDefinitionFormat GetRowAxisDefinitionFormat(XmlNode xmlNode, IXamlPlatform platform)
        {
            return GetAxisDefinitionFormat(xmlNode, platform.RowDefinitionsProperty);
        }

        public GridAxisDefinitionFormat GetAxisDefinitionFormat(XmlNode xmlNode, string axisName)
        {
            if (xmlNode is null)
            {
                throw new ArgumentNullException(nameof(xmlNode));
            }

            if (string.IsNullOrEmpty(axisName))
            {
                throw new ArgumentException("message", nameof(axisName));
            }

            if (xmlNode.HasAttribute(axisName))
            {
                if (!DoesAttributeContainAxisContent(xmlNode, axisName))
                {
                    return GridAxisDefinitionFormat.Undefined;
                }

                return GridAxisDefinitionFormat.Attribute;
            }

            if (xmlNode.HasChildNamed(xmlNode.Name.FullName + "." + axisName))
            {
                return GridAxisDefinitionFormat.Node;
            }

            return GridAxisDefinitionFormat.Undefined;
        }

        bool DoesAttributeContainAxisContent(XmlNode xmlNode, string axisName)
        {
            var attributeValue = xmlNode.GetAttributeByName(axisName);

            if (attributeValue is null || !attributeValue.HasValue) // Inner content is undefined.
            {
                return false;
            }

            if (ExpressionParserHelper.IsExpression(attributeValue.Value.Value)) // Inner attribute content is an expression, we will not be able to process this.
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveRowDefinitions(XmlNode xmlNode, IXamlPlatform platform)
        {
            return ResolveDefinitions(xmlNode, platform.RowDefinitionsProperty, platform.RowHeightProperty);
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveRowDefinitions(XmlNode xmlNode, GridAxisDefinitionFormat axisDefinitionFormat, IXamlPlatform platform)
        {
            return ResolveDefinitions(xmlNode, platform.RowDefinitionsProperty, platform.RowHeightProperty, axisDefinitionFormat);
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveColumnDefinitions(XmlNode xmlNode, IXamlPlatform platform)
        {
            return ResolveDefinitions(xmlNode, platform.ColumnDefinitionsProperty, platform.ColumnWidthProperty);
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveColumnDefinitions(XmlNode xmlNode, GridAxisDefinitionFormat axisDefinitionFormat, IXamlPlatform platform)
        {
            return ResolveDefinitions(xmlNode, platform.ColumnDefinitionsProperty, platform.ColumnWidthProperty, axisDefinitionFormat);
        }

        public string GetDimensionNameForAxis(string axisName, IXamlPlatform platform)
        {
            if (axisName == platform.RowDefinitionsProperty)
            {
                return platform.RowHeightProperty;
            }

            if (axisName == platform.ColumnDefinitionsProperty)
            {
                return platform.ColumnWidthProperty;
            }

            return string.Empty;
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveDefinitions(XmlNode xmlNode, string axisName, IXamlPlatform platform)
        {
            var format = GetAxisDefinitionFormat(xmlNode, axisName);
            var dimensionName = GetDimensionNameForAxis(axisName, platform);

            return ResolveDefinitions(xmlNode, axisName, dimensionName, format);
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveDefinitions(XmlNode xmlNode, string axisName, string dimensionName)
        {
            var format = GetAxisDefinitionFormat(xmlNode, axisName);

            return ResolveDefinitions(xmlNode, axisName, dimensionName, format);
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveDefinitions(XmlNode xmlNode, string axisName, GridAxisDefinitionFormat axisDefinitionFormat, IXamlPlatform platform)
        {
            var dimensionName = GetDimensionNameForAxis(axisName, platform);

            return ResolveDefinitions(xmlNode, axisName, dimensionName, axisDefinitionFormat);
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveDefinitions(XmlNode xmlNode, string axisName, string dimensionName, GridAxisDefinitionFormat axisDefinitionFormat)
        {
            if (xmlNode is null)
            {
                throw new ArgumentNullException(nameof(xmlNode));
            }

            if (string.IsNullOrEmpty(axisName))
            {
                throw new ArgumentException("message", nameof(axisName));
            }

            if (axisDefinitionFormat == GridAxisDefinitionFormat.Undefined)
            {
                return Array.Empty<IGridAxisDefinition>();
            }

            if (axisDefinitionFormat == GridAxisDefinitionFormat.Attribute)
            {
                return ResolveAttributeDefinedDefinitions(xmlNode, axisName, dimensionName);
            }

            return ResolveNodeDefinedDefinitions(xmlNode, axisName, dimensionName);
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveNodeDefinedDefinitions(XmlNode xmlNode, string axisName, string dimensionName)
        {
            if (xmlNode is null)
            {
                throw new ArgumentNullException(nameof(xmlNode));
            }

            if (string.IsNullOrEmpty(axisName))
            {
                throw new ArgumentException("message", nameof(axisName));
            }

            var propertySetter = xmlNode.GetChildNode(xmlNode.Name.FullName + "." + axisName);

            if (propertySetter is null || !propertySetter.HasChildren)
            {
                return Array.Empty<IGridAxisDefinition>();
            }

            var axisElementName = axisName.EndsWith("s") ? axisName.Substring(0, axisName.Length - 1) : axisName;

            var children = propertySetter.GetChildren(c => c.Name.FullName == axisElementName);

            if (!children.Any())
            {
                return Array.Empty<IGridAxisDefinition>();
            }

            var sizes = new List<IGridAxisDefinition>();
            foreach (var child in children)
            {
                var name = child.GetAttributeByName("x:Name");
                var dimension = child.GetAttributeByName(dimensionName);

                sizes.Add(new GridAxisDefinition(dimension?.Value?.Value, dimension?.Value?.Span, child.Span, children.IndexOf(child), GridAxisDefinitionFormat.Node, axisName, dimensionName, name?.Value?.Value ?? string.Empty)
                {
                    Syntax = child,
                });
            }

            return sizes;
        }

        public IReadOnlyList<IGridAxisDefinition> ResolveAttributeDefinedDefinitions(XmlNode xmlNode, string axisName, string dimensionName)
        {
            if (xmlNode is null)
            {
                throw new ArgumentNullException(nameof(xmlNode));
            }

            if (string.IsNullOrEmpty(axisName))
            {
                throw new ArgumentException("message", nameof(axisName));
            }

            if (!DoesAttributeContainAxisContent(xmlNode, axisName))
            {
                return Array.Empty<IGridAxisDefinition>();
            }

            var attributeValue = xmlNode.GetAttributeByName(axisName);
            var sizes = new List<IGridAxisDefinition>();

            var values = attributeValue.Value.Value.Split(',').ToList();

            var startOffset = attributeValue.Value.Span.Start;

            var index = 0;
            foreach (var value in values)
            {
                var fullSpan = new TextSpan(startOffset, value.Length + (index >= values.Count ? 0 : 1));

                sizes.Add(new GridAxisDefinition(value, new TextSpan(startOffset, value.Length), fullSpan, index, GridAxisDefinitionFormat.Attribute, axisName, dimensionName, string.Empty));

                startOffset += value.Length + 1;
                index++;
            }

            return sizes;
        }
    }
}