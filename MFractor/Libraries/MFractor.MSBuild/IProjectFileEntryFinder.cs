using System;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.MSBuild
{
    public interface IProjectFileEntryFinder
    {
        XmlNode FindProjectFile(Project project, IProjectFile projectFile);

        XmlNode FindProjectFile(string projectFilePath, IProjectFile projectFile);

        XmlNode FindProjectFile(XmlSyntaxTree syntaxTree, IProjectFile projectFile);
    }
}