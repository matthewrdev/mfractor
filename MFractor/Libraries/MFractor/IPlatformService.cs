namespace MFractor
{
    /// <summary>
    /// Allows inspection of the current platform.
    /// </summary>
	public interface IPlatformService
	{
        /// <summary>
        /// A human friendly name of the current platform.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The <see cref="DesktopPlatform"/>.
        /// </summary>
        DesktopPlatform DesktopPlatform { get; }

        /// <summary>
        /// Is the current platform Windows?
        /// </summary>
        /// <value><c>true</c> if is windows; otherwise, <c>false</c>.</value>
		bool IsWindows { get; }

        /// <summary>
        /// Is the current platform Linux?
        /// </summary>
        /// <value><c>true</c> if is linux; otherwise, <c>false</c>.</value>
		bool IsLinux { get; }

        /// <summary>
        /// Is the current platform OSX?
        /// </summary>
        /// <value><c>true</c> if is osx; otherwise, <c>false</c>.</value>
		bool IsOsx { get; }
    }
}

