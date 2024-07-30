namespace MFractor.Linker.CodeGeneration
{
    /// <summary>
    /// When preserving a type symbol, what members should be preserved when linking.
    /// </summary>
    public enum LinkerTypeMembersPreserveMode
    {
        /// <summary>
        /// Preserve all members.
        /// </summary>
        All,

        /// <summary>
        /// Preserve only methods.
        /// </summary>
        Methods,

        /// <summary>
        /// Preserve only fields.
        /// </summary>
        Fields,
    }
}
