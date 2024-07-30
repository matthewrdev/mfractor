using System.ComponentModel.Composition;

namespace MFractor.Xml
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXmlSyntaxWriter))]
    class XmlSyntaxWriter : IXmlSyntaxWriter
    {
        public string WriteNode(XmlNode xmlNode, string indent, IXmlFormattingPolicy policy, bool writeChildren, bool writeClosingTags, bool applyIndentToNode)
        {
            var nodeIndent = indent;
            var content = applyIndentToNode ? indent : "";

            content += $"<{xmlNode.Name.FullName}";

            var attrsOnLine = 0;
            if (xmlNode.HasAttributes)
            {
                content += " ";
                if (policy.AttributesInNewLine)
                {
                    content += policy.NewLineChars + nodeIndent + policy.AttributesIndentString;
                }

                var isFirstAttribute = true;
                var count = 0;
                foreach (var attr in xmlNode.Attributes)
                {
                    count++;
                    var attributeContent = WriteAttribute(attr, policy);

                    var attributeAlignmentIndent = nodeIndent + policy.AttributesIndentString;

                    if (policy.AlignAttributesToFirstAttribute)
                    {
                        attributeAlignmentIndent = nodeIndent + new string(' ', xmlNode.Name.FullName.Length
                                                                                + 1 // Leading '<'
                                                                                + 1); // Following space.
                    }

                    if (isFirstAttribute && policy.FirstAttributeOnNewLine)
                    {
                        content += policy.NewLineChars + attributeAlignmentIndent;
                    }

                    content += $"{attributeContent}";
                    attrsOnLine++;
                    if (attrsOnLine >= policy.MaxAttributesPerLine)
                    {
                        if (count != xmlNode.Attributes.Count)
                        {
                            content += policy.NewLineChars + attributeAlignmentIndent;
                        }
                    }
                    else
                    {
                        if (count != xmlNode.Attributes.Count)
                        {
                            content += " ";
                        }
                    }

                    isFirstAttribute = false;
                }
            }

            if (xmlNode.HasChildren
                || !xmlNode.IsSelfClosing
                || xmlNode.HasValue)
            {
                content += ">";

                if (writeChildren)
                {
                    if (xmlNode.HasChildren)
                    {
                        content += policy.NewLineChars;
                        var childCount = 0;
                        foreach (var n in xmlNode.Children)
                        {
                            content += WriteNode(n, nodeIndent + policy.ContentIndentString, policy, writeChildren, writeClosingTags, true);
                            childCount++;
                            if (childCount < xmlNode.Children.Count)
                            {
                                content += policy.NewLineChars;
                            }
                        }
                    }

                    if (xmlNode.HasValue)
                    {
                        content += xmlNode.Value;
                    }
                }

                if (writeClosingTags)
                {
                    if (!xmlNode.HasValue)
                    {
                        content += policy.NewLineChars + nodeIndent;
                    }
                    content += $"</{xmlNode.Name.FullName}>";
                }
            }
            else
            {
                if (writeClosingTags)
                {
                    content += policy.AppendSpaceBeforeSlashOnSelfClosingTag ? " />" : "/>";
                }
                else
                {
                    content += ">";
                }
            }

            return content;
        }

        public string WriteAttribute(XmlAttribute attribute, IXmlFormattingPolicy policy)
        {
            return attribute.Name.FullName + new string(' ', policy.SpacesBeforeAssignment) + "=" + new string(' ', policy.SpacesAfterAssignment) + "\"" + attribute.Value + "\"";
        }
    }
}
