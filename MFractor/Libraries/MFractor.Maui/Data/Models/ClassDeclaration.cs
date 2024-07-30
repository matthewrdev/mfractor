using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Data.Models
{
    public class ClassDeclaration : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The full symbol name for the class declaration. For example, "MyApp.Pages.MainPage".
        /// </summary>
        /// <value>The symbol.</value>
        public string MetaDataName { get; set; }

        /// <summary>
        /// The start location of the symbol declaration.
        /// </summary>
        /// <value>The start offset.</value>
        public int StartOffset { get; set; }

        /// <summary>
        /// The end location of the symbol declaration.
        /// </summary>
        /// <value>The end offset.</value>
        public int EndOffset { get; set; }

        /// <summary>
        /// The span of the static resource declaration syntax area.
        /// </summary>
        /// <value>The span.</value>
        public TextSpan Span => TextSpan.FromBounds(StartOffset, EndOffset);

        public override string ToString()
        {
            return MetaDataName;
        }
    }
}
