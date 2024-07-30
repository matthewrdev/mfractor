using MFractor.Workspace.Data.Models;

namespace MFractor.Fonts.Data.Models
{
    public class FontGlyphClassBinding : ProjectFileOwnedEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public string MetaType { get; set; }

        /// <summary>
        /// The file name of the font asset that 
        /// </summary>
        public string FontAssetFileName { get; set; }
    }
}
