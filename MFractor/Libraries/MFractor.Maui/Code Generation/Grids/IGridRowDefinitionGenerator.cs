using System;
using MFractor.Code.CodeGeneration;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Grids
{
    public interface IGridRowDefinitionGenerator : ICodeGenerator
    {
        string DefaultHeightValue { get; set; }

        XmlNode GenerateSyntax(string xmlns);

        XmlNode GenerateSyntax(string xmlns, string heightValue);
    }
}
