using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using MFractor.Text;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.Importing
{
    [InheritedExport]
    public interface ILocalisationValuesProvider
    {
        LocalisationImportSource[] SupportedImportSources { get; }

        string DisplayName { get; }

        ILocalisationFile GetDefaultFile(IEnumerable<ILocalisationFile> choices, string preference);

        bool CanProvide(Project project, out string errorMessage);
        bool CanProvide(DirectoryInfo directory, out string errorMessage);
        bool CanProvide(FileInfo file, out string errorMessage);

        IEnumerable<ILocalisationFile> RetrieveLocalisationFiles(Project project);
        IEnumerable<ILocalisationFile> RetrieveLocalisationFiles(DirectoryInfo directory);

        IReadOnlyDictionary<ILocalisationFile, IReadOnlyList<ILocalisationValue>> ProvideLocalisationValues(Project project);
        IReadOnlyDictionary<ILocalisationFile, IReadOnlyList<ILocalisationValue>> ProvideLocalisationValues(DirectoryInfo directory);
        IReadOnlyDictionary<ILocalisationFile, IReadOnlyList<ILocalisationValue>> ProvideLocalisationValues(IEnumerable<ILocalisationFile> files);

        IReadOnlyList<ILocalisationValue> ProvideLocalisationValues(FileInfo file);
        IReadOnlyList<ILocalisationValue> ProvideLocalisationValues(ITextProvider textProvider, CultureInfo culture);
        IReadOnlyList<ILocalisationValue> ProvideLocalisationValues(string content, CultureInfo culture);

        CultureInfo GetCultureInfo(string filePath);
        CultureInfo GetCultureInfo(FileInfo file);

    }
}
