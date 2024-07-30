namespace MFractor.iOS.Fonts
{
    public enum FontPlistEntryKind
    {
        /// <summary>
        /// Add only the declaration.
        /// <para/>
        /// EG: <string>FontFile.ttf</string>
        /// </summary>
        Declaration,

        /// <summary>
        /// Add the declaration and the infor
        /// </summary>
        ArrayDeclaration,

        UIAppFontsDeclaration,
    }
}
