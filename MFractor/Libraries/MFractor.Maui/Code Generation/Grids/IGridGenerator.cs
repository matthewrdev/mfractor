using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Grids
{
    public interface IGridGenerator : ICodeGenerator
    {
        bool IncludeColumnDefinitions { get; set; }

		bool IncludeRowDefinitions { get; set; }

		int DefaultRowsCount { get; set; }

		int DefaultColumnsCount { get; set; }

		XmlNode GenerateSyntax(IXamlFeatureContext context, IEnumerable<XmlNode> children);
    }
}
