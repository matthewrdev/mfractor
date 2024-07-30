using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Fonts;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using MFractor.Code;
using System.Linq;

namespace MFractor.Maui.CodeGeneration.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontFamilyOnPlatformGenerator))]
    class FontFamilyOnPlatformGenerator : XamlCodeGenerator, IFontFamilyOnPlatformGenerator
    {
        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        public override string Identifier => "com.mfractor.code_gen.xaml.font_family_onplatform";

        public override string Name => "FontFamily OnPlatform Generator";

        public override string Documentation => "Generates the XAML OnPlatform code needed for correctly referencing ";

        [ImportingConstructor]
        public FontFamilyOnPlatformGenerator(Lazy<IXmlSyntaxWriter> xmlSyntaxWriter)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
        }

        public string GenerateXaml(IFont font,
                                 string resourceKey,
                                 IEnumerable<Project> projects,
                                 string childIndent = "  ")
        {
            return GenerateXaml(font.FilePath, font.Name, font.PostscriptName, font.Style, resourceKey, font.FamilyName, projects, childIndent);
        }

        public string GenerateXaml(string fontFileName,
                                 string fontName,
                                 string postscriptName,
                                 string fontStyle,
                                 string resourceKey,
                          string typographicFamilyName,
                                 IEnumerable<Project> projects,
                                 string childIndent = "  ")
        {

            return GenerateXaml(fontFileName, fontName, postscriptName, fontStyle, resourceKey, typographicFamilyName, projects.Select(p => p.GetPlatform()), childIndent);
        }

        string CreateAppleFontLookup(string postscriptName, string fontName)
        {
            if (!string.IsNullOrEmpty(postscriptName))
            {
                return postscriptName;
            }

            return fontName;
        }

        public string GenerateXaml(IFont font, string resourceKey, IEnumerable<PlatformFramework> platforms, string childIndent = "  ")
        {
            return GenerateXaml(font.FilePath, font.Name, font.PostscriptName, font.Style, resourceKey, font.FamilyName, platforms, childIndent);
        }

        public string GenerateXaml(string fontFileName, string fontName, string postscriptName, string fontStyle, string resourceKey, string typographicFamilyName, IEnumerable<PlatformFramework> platforms, string childIndent = "  ")
        {
            var node = Generate(fontFileName, fontName, postscriptName, fontStyle, resourceKey, typographicFamilyName, platforms);

            var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            return XmlSyntaxWriter.WriteNode(node, string.Empty, policy, true, true, true);
        }

        public XmlNode Generate(IFont font, string resourceKey, IEnumerable<Project> projects)
        {
            return Generate(font.FilePath, font.Name, font.PostscriptName, font.Style, resourceKey, font.FamilyName, projects);
        }

        public XmlNode Generate(string fontFilePath, string fontName, string postscriptName, string fontStyle, string resourceKey, string typographicFamilyName, IEnumerable<Project> projects)
        {
            return Generate(fontFilePath, fontName, postscriptName, fontStyle, resourceKey, typographicFamilyName, projects.Select(p => p.GetPlatform()));
        }

        public XmlNode Generate(IFont font, string resourceKey, IEnumerable<PlatformFramework> platforms)
        {
            return Generate(font.FilePath, font.Name, font.PostscriptName, font.Style, resourceKey, font.FamilyName, platforms);
        }

        public XmlNode Generate(string fontFilePath, string fontName, string postscriptName, string fontStyle, string resourceKey, string typographicFamilyName, IEnumerable<PlatformFramework> platforms)
        {
            var onPlatform = new XmlNode("OnPlatform");
            onPlatform.AddAttribute("x:TypeArguments", "x:String");

            if (!string.IsNullOrEmpty(resourceKey))
            {
                onPlatform.AddAttribute("x:Key", resourceKey);
            }

            var fontAssetName = Path.GetFileName(fontFilePath);

            foreach (var p in platforms.Distinct())
            {
                var platform = "";
                var value = "";

                if (p == PlatformFramework.Android)
                {
                    platform = "Android";
                    value = fontAssetName + "#" + fontStyle;
                }
                else if (p == PlatformFramework.iOS)
                {
                    platform = "iOS";
                    value = CreateAppleFontLookup(postscriptName, fontName);
                }
                else if (p == PlatformFramework.MacOS)
                {
                    platform = "macOS";
                    value = CreateAppleFontLookup(postscriptName, fontName);
                }
                else if (p == PlatformFramework.UWP)
                {
                    platform = "UWP";
                    value = "/Assets/Fonts/" + fontAssetName + "#" + typographicFamilyName;
                }

                var on = new XmlNode("On");
                on.AddAttribute("Platform", platform);
                on.AddAttribute("Value", value);
                on.IsSelfClosing = true;
                onPlatform.AddChildNode(on);
            }

            return onPlatform;
        }
    }
}