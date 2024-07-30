using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.Resources
{
    /// <summary>
    /// Inserts a resource entry into a files resources.
    /// </summary>
    public interface IInsertResourceEntryGenerator : ICodeGenerator
    {
        IReadOnlyList<IWorkUnit> Generate(Project project, string filePath, XmlSyntaxTree xmlSyntaxTree, XmlNode node);
        IReadOnlyList<IWorkUnit> Generate(Project project, string filePath, XmlNode node);

        IReadOnlyList<IWorkUnit> Generate(IProjectFile projectFile, XmlSyntaxTree xmlSyntaxTree, XmlNode node);
        IReadOnlyList<IWorkUnit> Generate(IProjectFile projectFile, XmlNode node);
    }
}
