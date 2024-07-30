using System;
using System.Collections.Generic;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.MSBuild
{
    public interface IItemGroupFinder
    {
        IEnumerable<XmlNode> FindItemGroups(Project project);

        IEnumerable<XmlNode> FindItemGroups(string projectFilePath);

        IEnumerable<XmlNode> FindItemGroups(XmlSyntaxTree syntaxTree);
    }
}