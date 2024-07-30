using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide
{
    [InheritedExport]
    public interface IFileCreationPostProcessor
    {
        bool CanPostProcess(FileInfo fileInfo, Project project);

        string PostProcess(string content, FileInfo fileInfo, Project project);
    }
}
