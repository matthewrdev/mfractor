using System;
using System.Collections.Generic;
using MFractor.Attributes;
using MFractor.Code.CodeGeneration;
using MFractor.Maui.XamlPlatforms;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Styles
{
    /// <summary>
    /// When a style has a parent style, what kind of parent style is it?
    /// </summary>
    public enum ParentStyleType
    {
        /// <summary>
        /// Set the parent style using the BasedOn attribute.
        /// </summary>
        [Description("BasedOn")]
        BasedOn,

        /// <summary>
        /// Set the parent style using the BaseResourceKey attribute.
        /// </summary>
        [Description("BaseResourceKey")]
        BaseResourceKey,
    }

    /// <summary>
    /// Generates a XAML style.
    /// </summary>
    public interface IStyleGenerator : ICodeGenerator
    {
        /// <summary>
        /// Generates the XAML code for a style.
        /// </summary>
        string GenerateCode(IXamlPlatform platform,
                            string styleName, 
                            string targetType,
                            string targetTypePrefix,
                            string parentStyleName, 
                            ParentStyleType parentStyleType,
                            IReadOnlyDictionary<string, string> properties);

        /// <summary>
        /// Generates an XML node for a style.
        /// </summary>
        XmlNode GenerateSyntax(IXamlPlatform platform,
                               string styleName,
                               string targetType,
                               string targetTypePrefix,
                               string parentStyleName,
                               ParentStyleType parentStyleType,
                               IReadOnlyDictionary<string, string> properties);
    }
}
