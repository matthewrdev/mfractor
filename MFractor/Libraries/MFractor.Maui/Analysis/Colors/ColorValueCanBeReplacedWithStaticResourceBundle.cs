using System.Collections.Generic;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Analysis.Colors
{
    public class ColorValueCanBeReplacedWithStaticResourceBundle
    {
        public ColorValueCanBeReplacedWithStaticResourceBundle(IReadOnlyList<ColorDefinition> matchingColorDefinitions)
        {
            MatchingColorDefinitions = matchingColorDefinitions;
        }

        public IReadOnlyList<ColorDefinition> MatchingColorDefinitions { get; }
    }
}
