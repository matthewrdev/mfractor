using System;
using System.ComponentModel.Composition;
using System.Security;
using MFractor.Code.CodeGeneration;
using MFractor.Configuration.Attributes;

using MFractor.Xml;

namespace MFractor.Localisation.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IResXEntryGenerator))]
    class ResXEntryGenerator : CodeGenerator, IResXEntryGenerator
    {
        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        public override string[] Languages { get; } = new string[] { "RESX" };

        public override string Identifier => "com.mfractor.code_gen.resx.generate_resx_entry";

        public override string Name => "Generate ResX Entry";

        public override string Documentation => "Generates a single ResX resource entry element containing the key, value and comment.";

        [ExportProperty("When no comment for the new ResX entry has been provided, should an empty comment tag be included?")]
        public bool IncludeCommentWhenEmpty { get; set; } = false;

        [ImportingConstructor]
        public ResXEntryGenerator(Lazy<IXmlSyntaxWriter> xmlSyntaxWriter)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
        }

        public string GenerateCode(string key, string value, string comment, string indent)
        {
            var node = GenerateSyntax(key, value, comment);

            var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            return XmlSyntaxWriter.WriteNode(node, indent, policy, true, true, true);
        }

        public XmlNode GenerateSyntax(string key, string value, string comment)
        {
            var children = new System.Collections.Generic.List<XmlNode>()
                {
                    new XmlNode()
                    {
                        Name = new XmlName("value"),
                        Value = SecurityElement.Escape(value),
                        HasClosingTag = true,
                    }
            };

            if (!string.IsNullOrEmpty(comment) || IncludeCommentWhenEmpty)
            {
                children.Add(new XmlNode()
                {
                    Name = new XmlName("comment"),
                    Value = SecurityElement.Escape(comment),
                    IsSelfClosing = string.IsNullOrEmpty(comment),
                });
            }

            return new XmlNode()
            {
                Name = new XmlName("data"),
                Attributes = new System.Collections.Generic.List<XmlAttribute>()
                {
                    new XmlAttribute("name", key),
                    new XmlAttribute("xml:space", "preserve")
                },
                Children = children,
            };
        }
    }
}
