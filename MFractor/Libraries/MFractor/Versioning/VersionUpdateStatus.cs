namespace MFractor
{
    /// <summary>
    /// When MFractor starts again between sessions, this enum specifies the kind of version upgrade that occured.
    /// </summary>
    public enum VersionUpdateStatus
    {
        /// <summary>
        /// There was no version update.
        /// </summary>
        None,

        /// <summary>
        /// The version update occured on the patch release.
        /// </summary>
        VersionUpgrade,

        /// <summary>
        /// The version update was on the major or minor version number.
        /// </summary>
        SignificantVersionUpgrade,
    }
}
