using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation.Data.Models
{
    /// <summary>
    /// A localisation value that exists within a .resx file.
    /// </summary>
    public class ResXLocalisationEntry : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The 
        /// </summary>
        public int ResXDefinitionKey { get; set; }

        public string CultureCode { get; set; }

        public string Value { get; set; }

        public string SearchValue { get; set; }

        public int ValueStartOffset { get; set; }

        public int ValueEndOffset { get; set; }

        public TextSpan ValueSpan
        {
            get => TextSpan.FromBounds(ValueStartOffset, ValueEndOffset);
            set
            {
                ValueStartOffset = value.Start;
                ValueEndOffset = value.End;
            }
        }

        public int KeyStartOffset { get; set; }

        public int KeyEndOffset { get; set; }

        public TextSpan KeySpan
        {
            get => TextSpan.FromBounds(KeyStartOffset, KeyEndOffset);
            set
            {
                KeyStartOffset = value.Start;
                KeyEndOffset = value.End;
            }
        }
    }
}
