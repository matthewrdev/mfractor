using System;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Fonts.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that launches the font importer.
    /// </summary>
    public class ImportFontWorkUnit : WorkUnit
    {
        /// <summary>
        /// The solution that the font should be imported into.
        /// </summary>
        public Solution Solution { get; set; }

        /// <summary>
        /// The project to injec
        /// </summary>
        public ProjectIdentifier ProjectIdentifier { get; set; }

        /// <summary>
        /// An action to call after the font has been imported.
        /// </summary>
        public Action<FontImportResult> ImportAction { get; set; }

        /// <summary>
        /// When the font is imported, should an App.xaml entry automatically be added into the <see cref="ProjectIdentifier"/>.
        /// </summary>
        public bool InjectFontIntoXaml { get; set; } = true;
    }
}
