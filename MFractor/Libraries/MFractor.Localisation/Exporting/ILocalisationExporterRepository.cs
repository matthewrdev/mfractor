using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Localisation.Exporting
{
    public interface ILocalisationExporterRepository : IPartRepository<ILocalisationExporter>
    {
        IReadOnlyList<ILocalisationExporter> LocalisationExporters { get; }
    }
}
