using System;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Data.Models
{
    public class StyleSetter : ProjectFileOwnedEntity
    {
        public int StyleDefinitionKey { get; set; }

        public string Property { get; set; }

        public int PropertyOffset { get; set; }

        public int PropertyLength { get; set; }

        
        public TextSpan PropertySpan => TextSpan.FromBounds(PropertyOffset, PropertyOffset + PropertyLength);

        public string Value { get; set; }

        public int ValueOffset { get; set; }

        public int ValueLength { get; set; }

        
        public TextSpan ValueSpan => TextSpan.FromBounds(ValueOffset, ValueOffset + ValueLength);
    }
}
