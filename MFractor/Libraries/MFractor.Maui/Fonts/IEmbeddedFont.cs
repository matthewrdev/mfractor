using MFractor.Fonts;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Fonts
{
    /// <summary>
    /// An <see cref="IFont"/> that is defined using the ExportFontAttribute on an assembly.
    /// <para/>
    /// See: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/text/fonts
    /// </summary>
    public interface IEmbeddedFont
    {
        /// <summary>
        /// The name of the font asset, as defined by the ExportFontAttribute.FontFileName
        /// </summary>
        string FontFileName { get; }

        /// <summary>
        /// The name of this font asset.
        /// <para/>
        /// This is the <see cref="FontFileName"/> without the <see cref="FontFileExtension"/>
        /// </summary>
        string FontName { get; }

        /// <summary>
        /// The file extension of the font.
        /// </summary>
        string FontFileExtension { get; }

        /// <summary>
        /// The alias of the font asset, as defined by the ExportFontAttribute.FontFileName
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Does this font define an alias?
        /// </summary>
        bool HasAlias { get; }

        /// <summary>
        /// When refering to the font in XAML, what is the name used to reference this font asset?
        /// </summary>
        string LookupName { get; }

        /// <summary>
        /// The project that owns this font asset.
        /// </summary>
        Project CompilationProject { get; }

        /// <summary>
        /// The font asset that this exported font references.
        /// </summary>
        IFont Font { get; }
    }
}