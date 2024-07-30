using System;
using System.Collections.Generic;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Grids
{
    public interface IDeleteGridRowColumnGenerator
    {
        IReadOnlyList<IWorkUnit> DeleteGridRow(IXamlPlatform platform, XmlNode grid, int deletionIndex, string filePath);

        IReadOnlyList<IWorkUnit> DeleteGridColumn(IXamlPlatform platform, XmlNode grid, int deletionIndex, string filePath);
    }
}
