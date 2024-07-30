using System;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.Fonts.Data.Models
{
    /// <summary>
    /// A font asset such as a 
    /// </summary>
    public class FontFileAsset : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The name of the font asset.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName { get; set; }

        /// <summary>
        /// The file name of this font asset, excluding any directory or path components
        /// </summary>
        /// <value>The full name.</value>
        public string FileName { get; set; }

        /// <summary>
        /// The style of the font asset.
        /// <para/>
        /// For example, Bold, Bold Italic, Italic etc.
        /// </summary>
        /// <value>The style.</value>
        public string Style { get; set; }

        /// <summary>
        /// The Postscript name of the font asset.
        /// </summary>
        /// <value>The name of the postscript.</value>
        public string PostscriptName { get; set; }

        /// <summary>
        /// The Typographic Family Name of this font.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Is this font a web font?
        /// </summary>
        public bool IsWebFont { get; set; }
    }
}
