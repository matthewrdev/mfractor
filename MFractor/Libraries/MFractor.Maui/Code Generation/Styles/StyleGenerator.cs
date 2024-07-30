using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.XamlPlatforms;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Styles
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IStyleGenerator))]
    class StyleGenerator : XamlCodeGenerator, IStyleGenerator
    {
        readonly IXmlSyntaxWriter xmlSyntaxWriter;

        public override string Identifier => "com.mfractor.code_gen.xaml.style";

        public override string Name => "Generate XAML Style";

        public override string Documentation => "Generates a new XAML style that can be applied to a `VisualElement`.";

        [ImportingConstructor]
        public StyleGenerator(IXmlSyntaxWriter xmlSyntaxWriter)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
        }

        public string GenerateCode(IXamlPlatform platform, string styleName, string targetType, string targetTypePrefix, string parentStyleName, ParentStyleType parentStyleType, IReadOnlyDictionary<string, string> properties)
        {
            var syntax = GenerateSyntax(platform, styleName, targetType, targetTypePrefix, parentStyleName, parentStyleType, properties);

            var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            return xmlSyntaxWriter.WriteNode(syntax, string.Empty, policy, true, true, true);
        }

        public XmlNode GenerateSyntax(IXamlPlatform platform, string styleName, string targetType, string targetTypePrefix, string parentStyleName, ParentStyleType parentStyleType, IReadOnlyDictionary<string, string> properties)
        {
            var node = new XmlNode(platform.Style.Name);

            if (string.IsNullOrEmpty(targetType))
            {
                throw new ArgumentException("message", nameof(targetType));
            }

            if (!string.IsNullOrEmpty(styleName))
            {
                node.AddAttribute("x:Key", styleName);
            }

            targetType = targetType.Split('.').Last();

            var targetTypeValue = string.IsNullOrEmpty(targetTypePrefix) ? targetType : $"{targetTypePrefix}:{targetType}";

            node.AddAttribute("TargetType", targetTypeValue);

            if (!string.IsNullOrEmpty(parentStyleName))
            {
                switch (parentStyleType)
                {
                    case ParentStyleType.BasedOn:
                        node.AddAttribute("BasedOn", "{" + platform.StaticResourceExtension.MarkupExpressionName + " " + parentStyleName + "}");
                        break;
                    case ParentStyleType.BaseResourceKey:
                        node.AddAttribute("BaseResourceKey", parentStyleName);
                        break;
                }
            }

            foreach (var prop in properties)
            {
                var setter = new XmlNode("Setter");
                setter.AddAttribute("Property", prop.Key);
                setter.AddAttribute("Value", prop.Value);
                setter.IsSelfClosing = true;

                node.AddChildNode(setter);
            }

            return node;
        }
    }
}