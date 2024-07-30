using System;
using MFractor.Attributes;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.CSharp.Models
{
    /// <summary>
    /// Represents the LangVersion element (an item of PropertyGroup) of an C# project file.
    /// </summary>
    public class LangVersionProperty
    {
        /// <summary>
        /// The name of the Xml Element when serializing or deserializing.
        /// </summary>
        public const string ElementName = "LangVersion";

        public string Value { get; set; }

        public LangVersionProperty(string value)
        {
            Value = value;
        }

        public XmlNode GetNode() => new XmlNode(ElementName)
        {
            Value = Value,
        };
    }

    /// <summary>
    /// Enumerates C# Language Versions.
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version" />
    public enum CSharpVersion
    {
        [Description("default")]
        Default,

        [Description("preview")]
        Preview,

        [Description("7.0")]
        V7,

        [Description("7.1")]
        V7_1,

        [Description("7.2")]
        V7_2,

        [Description("7.3")]
        V7_3,

        [Description("8.0")]
        V8,

        [Description("9.0")]
        V9,
    }

    public static class CSharpVersionExtensions
    {
        public static string GetValue(this CSharpVersion version) =>
            EnumHelper.GetEnumDescription(version);
    }

    public static class XmlNodeLangVersionExtensions
    {
        public static LangVersionProperty ToLangVersion(this XmlNode node)
        {
            if (node is null)
            {
                return null;
            }
            if (node.Name.FullName != "LangVersion")
            {
                return null;
            }
            return new LangVersionProperty(node.Value);
        }
    }
}
