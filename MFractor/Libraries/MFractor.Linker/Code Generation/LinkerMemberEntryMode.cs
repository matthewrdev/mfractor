namespace MFractor.Linker.CodeGeneration
{
    /// <summary>
    /// The type of linker entry that should be generated for a method.
    /// </summary>
    public enum LinkerMemberEntryMode
    {
        /// <summary>
        /// Creates a new linker entry for the method that excludes it by name.
        /// </summary>
        Name,

        /// <summary>
        /// Creates a new linker entry for the method that excludes it by it's method signature.
        /// </summary>
        Signature,
    }
}
