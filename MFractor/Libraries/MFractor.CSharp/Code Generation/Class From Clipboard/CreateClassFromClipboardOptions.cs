using System;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.CodeGeneration.ClassFromClipboard
{
    public class CreateClassFromClipboardOptions
    {
        public CreateClassFromClipboardOptions(string name,
                                               Project project,
                                               string folderPath,
                                               NamespaceMode namespaceMode,
                                               string customNamespace = "")
        {
            Name = name;
            Project = project;
            FolderPath = folderPath;
            NamespaceMode = namespaceMode;
            CustomNamespace = customNamespace;
        }

        public string Name { get; }

        public Project Project { get; }

        public string FolderPath { get; }

        public NamespaceMode NamespaceMode { get; }

        public string CustomNamespace { get; }
    }
}