using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Grids
{
    public interface IInsertGridRowColumnGenerator : ICodeGenerator
    {
        IReadOnlyList<IWorkUnit> InsertGridRow(IXamlPlatform platform, XmlNode grid, int insertionIndex, InsertionLocation insertionLocation, string filePath, string unit);

        IReadOnlyList<IWorkUnit> InsertGridColumn(IXamlPlatform platform, XmlNode grid, int insertionIndex, InsertionLocation insertionLocation, string filePath, string unit);
    }
}
