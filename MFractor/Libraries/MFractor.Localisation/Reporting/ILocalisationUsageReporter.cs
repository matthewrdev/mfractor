using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Configuration;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.Reporting
{
    [InheritedExport]
    public interface ILocalisationUsageReporter
    {
        IReadOnlyDictionary<ILocalisationFile, List<LocalisationUsage>> Report(Project project);
        IReadOnlyDictionary<ILocalisationFile, List<LocalisationUsage>> Report(Project project, string filePath);
    }
}
