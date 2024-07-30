using Microsoft.CodeAnalysis;

namespace MFractor.Fonts
{
    /// <summary>
    /// A font asset, such as an .ttf or .otf file, that is on disk.
    /// <para/>
    /// The <see cref="IFont"/> interface exposes meta-data about a font including its name, style, postscript, and family name.
    /// </summary>
    public interface IFont
    {
        /// <summary>
        /// The file name of this font asset, excluding the directpry/full file path.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// The full file path on disk for the font asset.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// The name of the font, as specified in its header content.
        /// <para/>
        /// This may not match the <see cref="FileName"/>;
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The full name of the font asset, as specified in its header content.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// The style of this font asset.
        /// <para/>
        /// EG: Regular, bold etc
        /// </summary>
        string Style { get; }

        /// <summary>
        /// The postscript name of this font asset.
        /// </summary>
        string PostscriptName { get; }

        /// <summary>
        /// The family name for this font asset.
        /// </summary>
        string FamilyName { get; }

        /// <summary>
        /// If this font asset is a web font or a desktop font.
        /// <para/>
        /// Webfonts include additional meta-data that provides names for each glyph in the font; typically desktop fonts do not include 
        /// </summary>
        bool IsWebFont { get; }
    }
}
