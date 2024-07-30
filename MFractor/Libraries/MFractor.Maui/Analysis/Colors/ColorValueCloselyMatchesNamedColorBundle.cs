using System.Collections.Generic;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Analysis.Colors
{
    public class ColorValueCloselyMatchesNamedColorBundle
    {
        public ColorValueCloselyMatchesNamedColorBundle(IReadOnlyList<string> matchingColrs)
        {
            MatchingColors = matchingColrs;
        }

        public IReadOnlyList<string> MatchingColors { get; }
    }
}
