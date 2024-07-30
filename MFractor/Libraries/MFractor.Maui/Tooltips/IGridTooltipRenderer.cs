using System;
using MFractor.Maui.XamlPlatforms;
using MFractor.Xml;

namespace MFractor.Maui.Tooltips
{
    public interface IGridTooltipRenderer
    {
        string CreateColumnSpanTooltip(XmlAttribute attribute, XmlNode gridNode, IXamlPlatform platform);
        string CreateColumnTooltip(XmlAttribute attribute, XmlNode gridNode, IXamlPlatform platform);
        string CreateRowSpanTooltip(XmlAttribute attribute, XmlNode gridNode, IXamlPlatform platform);
        string CreateRowTooltip(XmlAttribute attribute, XmlNode gridNode, IXamlPlatform platform);
    }
}