using System;
using MFractor.Data;
using MFractor.Data.Models;

namespace MFractor.Localisation.Data.Models
{
    public class ResXLocalisationDefinition : GCEntity
    {
        public string Key { get; set; }

        public string SearchName { get; set; }
    }
}
