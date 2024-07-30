using System;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Data.Models
{
    public class AutomationIdDeclaration : ProjectFileOwnedEntity
    {
        public string Name { get; set; }

        public int StartOffset { get; set; }

        public int EndOffset { get; set; }

        
        public TextSpan Span => TextSpan.FromBounds(StartOffset, EndOffset);

        public string ParentSymbolName { get; set; }

        public string ParentSymbolNamespace { get; set; }

        
        public string ParentMetaDataName
        {
            get
            {
                if (string.IsNullOrEmpty(ParentSymbolName))
                {
                    return "";
                }

                if (string.IsNullOrEmpty(ParentSymbolNamespace))
                {
                    return "";
                }

                return ParentSymbolNamespace + "." + ParentSymbolName;
            }
        }
    }
}
