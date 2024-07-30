using System;
using MFractor.Configuration;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.ValueConversion
{
    public interface IValueConversionSettings : IConfigurable
    {
        string Namespace { get; set; }

        string Folder { get; set; }

        string DefaultConverterXmlns { get; set; }

        string GetItemFilePath(string fileName);

        string CreateConvertersClrNamespace(ProjectIdentifier project, string folderPath = null);
    }
}
