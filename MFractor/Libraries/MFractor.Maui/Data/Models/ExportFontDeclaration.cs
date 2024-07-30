using System;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.Maui.Data.Models
{
    public class ExportFontDeclaration : ProjectFileOwnedEntity
    {
        public string FontFileName { get; set; }

        public string EmbeddedResourceId { get; set; }

        public string Alias { get; set; }
    }
}
