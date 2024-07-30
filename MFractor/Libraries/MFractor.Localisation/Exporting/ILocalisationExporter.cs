using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace MFractor.Localisation.Exporting
{
    [InheritedExport]
    public interface ILocalisationExporter
    {
        void Write(string filePath, IReadOnlyDictionary<ILocalisationFile, List<ILocalisationValue>> localisations);

        void Write(Stream stream, IReadOnlyDictionary<ILocalisationFile, List<ILocalisationValue>> localisations);

        string Write(IReadOnlyDictionary<ILocalisationFile, List<ILocalisationValue>> localisations);
    }
}
