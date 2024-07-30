using System;
using MFractor.Code.CodeGeneration;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Grids
{
    public interface IGridColumnDefinitionGenerator : ICodeGenerator
    {
        string DefaultWidthValue { get; set; }

        XmlNode GenerateSyntax(string xmlns);

        XmlNode GenerateSyntax(string xmlns, string widthValue);
    }
}
