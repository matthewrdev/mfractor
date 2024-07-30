using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Localisation.Exporting
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILocalisationExporterRepository))]
    class LocalisationExporterRepository : PartRepository<ILocalisationExporter>, ILocalisationExporterRepository
    {
        [ImportingConstructor]
        public LocalisationExporterRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<ILocalisationExporter> LocalisationExporters => Parts;
    }
}