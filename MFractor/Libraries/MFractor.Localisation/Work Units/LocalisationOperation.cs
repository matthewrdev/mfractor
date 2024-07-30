using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.CodeSnippets;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.WorkUnits
{
	public class LocalisationOperation
	{
        public ILocalisableString Target { get; }

        public Project Project { get; }

        public string Key { get; }

        public string Value { get; }

        public string Comment { get; }

        public IReadOnlyList<string> AllResourceFiles { get; }

        public IReadOnlyList<string> SelectedResourceFiles { get; }

        public LocalisationOperation(ILocalisableString target,
                                     Project project,
                                     string key,
                                     string value,
                                     string comment,
                                     IEnumerable<string> allResourceFiles,
                                     IEnumerable<string> selectedResourceFiles)
		{
            Key = key;
			Value = value;
			Comment = comment;
            AllResourceFiles = (allResourceFiles ?? Enumerable.Empty<string>()).ToList();
            SelectedResourceFiles = (selectedResourceFiles ?? Enumerable.Empty<string>()).ToList();
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Project = project ?? throw new ArgumentNullException(nameof(project));
        }

        public bool IsValid => !string.IsNullOrEmpty(Key);

        public bool ShouldCreatedDefaultLocalisationFile => !AllResourceFiles.Any();
    }
}
