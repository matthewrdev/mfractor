using System;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for locating the best suggestion from a collection of strings.
    /// </summary>
    public static class SuggestionHelper
    {
        /// <summary>
        /// The default comparison heuristic when finding a best suggestion.
        /// </summary>
        public const int DefaultBestSuggestionDistance = 3;

        /// <summary>
        /// Finds the best suggestion from <paramref name="suggestions"/> for the given <paramref name="input"/> or an empty string if there is no match.
        /// </summary>
        /// <returns>The best suggestion.</returns>
        /// <param name="input">Input.</param>
        /// <param name="suggestions">Suggestions.</param>
        /// <param name="bestSuggestionDistance">Best suggestion distance.</param>
        public static string FindBestSuggestion(string input, IEnumerable<string> suggestions, int bestSuggestionDistance = DefaultBestSuggestionDistance)
        {
            var suggestion = string.Empty;

            if  (suggestions == null || !suggestions.Any())
            {
                return suggestion;
            }

            var bestDistance = int.MaxValue;
            foreach (var s in suggestions)
            {
                if (s == null)
                {
                    continue;
                }

                var distance = LevenshteinDistanceHelper.Compute(s, input);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    suggestion = s;
                }
            }

            return bestDistance < bestSuggestionDistance ? suggestion : "";
        }
    }
}