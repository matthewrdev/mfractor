using System;
using System.Collections.Generic;
using System.Text;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.iOS.Data.Models
{
    public class BundleDetails : ProjectFileOwnedEntity
    {
        public string BundleIdentifier { get; set; }
    }
}
