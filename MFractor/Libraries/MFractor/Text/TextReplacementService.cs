using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Progress;

namespace MFractor.Text
{
    public abstract class TextReplacementService : ITextReplacementService
    {
        public void ApplyTextReplacements(IReadOnlyDictionary<string, IReadOnlyList<ITextReplacement>> replacements, IProgressMonitor progressMonitor)
        {
            var workUnit = 0;
            var totalWork = replacements.Count;

            progressMonitor.SetProgress("Applying code changes...", workUnit, totalWork);
            foreach (var fileReplacements in replacements)
            {
                var fileName = fileReplacements.Key;

                workUnit++;

                progressMonitor?.SetProgress("Applying code changes...", workUnit, totalWork);

                ApplyTextReplacements(fileName, fileReplacements.Value, progressMonitor);
            }
        }

        public void ApplyTextReplacements(IReadOnlyList<ITextReplacement> replacements, IProgressMonitor progressMonitor)
        {
            var replacementsMap = new Dictionary<string, List<ITextReplacement>>();

            foreach (var rc in replacements)
            {
                if (!replacementsMap.ContainsKey(rc.FilePath))
                {
                    replacementsMap[rc.FilePath] = new List<ITextReplacement>();
                }
                replacementsMap[rc.FilePath].Add(rc);
            }

            ApplyTextReplacements(replacementsMap.ToDictionary(kp => kp.Key, kp => (IReadOnlyList<ITextReplacement>)kp.Value), progressMonitor);
        }

        public abstract void ApplyTextReplacements(string fileName, IReadOnlyList<ITextReplacement> replacements, IProgressMonitor progressMonitor);
    }
}