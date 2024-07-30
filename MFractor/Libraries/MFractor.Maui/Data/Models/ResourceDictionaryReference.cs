using System;
using System.Diagnostics;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.Maui.Data.Models
{
    [DebuggerDisplay("{MetaTypeName} - {ReferenceKind}")]
    public class ResourceDictionaryReference : ProjectFileOwnedEntity
    {
        public string SymbolName { get; set; }

        public string SymbolNamespace { get; set; }

        public DictionaryReferenceKind ReferenceKind { get; set; } = DictionaryReferenceKind.Symbol;

        public string FileName { get; set; }

        public string MetaTypeName
        {
            get
            {
                if (string.IsNullOrEmpty(SymbolName))
                {
                    return "";
                }

                if (string.IsNullOrEmpty(SymbolNamespace))
                {
                    return "";
                }

                return SymbolNamespace + "." + SymbolName;
            }
            
        }
    }
}
