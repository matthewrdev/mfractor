using System.Collections.Generic;
using MFractor.Maui.Styles;

namespace MFractor.Maui.Analysis.Styles
{
    class ElementCanUseAvailableStyleBundle
    {
        public ElementCanUseAvailableStyleBundle(IReadOnlyList<IStyle> styles)
        {
            Styles = styles;
        }

        public IReadOnlyList<IStyle> Styles { get; }
    }
}
