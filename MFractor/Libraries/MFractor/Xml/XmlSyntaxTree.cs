using System;
using System.Linq;
using MFractor.Work.WorkUnits;
using Microsoft.Language.Xml;

namespace MFractor.Xml
{
    /// <summary>
    /// Represents a simplified Xml syntax tree.
    /// <para/>
    /// Use RawSyntax to get the full fidelity XML syntax tree.
    /// </summary>
    public class XmlSyntaxTree : XmlSyntax, IXmlSyntaxTree
    {
        public XmlNode Root { get; set; }
    }

    public static class XmlSyntaxTreeProjectExtensions
    {
        public static bool IsDotNetProject(this XmlSyntaxTree syntaxTree) =>
            syntaxTree.Root.Name.FullName == "Project" && syntaxTree.Root.HasChildren;

        public static bool IsNetStandardProject(this XmlSyntaxTree syntaxTree)
        {
            if (!syntaxTree.IsDotNetProject())
            {
                return false;
            }
            
            var targetFramework = syntaxTree.Root.Children
                .Where(c => c.Name.FullName == "PropertyGroup" &&
                            c.Children.Any(ch => ch.Name.FullName == "TargetFramework"))
                .FirstOrDefault()?
                .Children
                .FirstOrDefault(c => c.Name.FullName == "TargetFramework");

            return targetFramework is not null && targetFramework.Value.Contains("netstandard");
        }
    }
}

