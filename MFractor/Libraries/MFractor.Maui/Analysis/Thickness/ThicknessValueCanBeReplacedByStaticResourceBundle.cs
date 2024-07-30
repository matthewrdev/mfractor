using System.Collections.Generic;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Analysis.Thickness
{
    public class ThicknessValueCanBeReplacedByStaticResourceBundle
    {
        public ThicknessValueCanBeReplacedByStaticResourceBundle(IReadOnlyList<ThicknessDefinition> matchingThicknessDefinitions)
        {
            MatchingThicknessDefinitions = matchingThicknessDefinitions;
        }

        public IReadOnlyList<ThicknessDefinition> MatchingThicknessDefinitions { get; }
    }
}
