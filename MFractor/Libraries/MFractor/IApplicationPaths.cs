using System;
namespace MFractor
{
    /// <summary>
    /// The paths that this application uses.
    /// <para/>
    /// If subsystems need to create assets inside MFractors working data folder, use <see cref="ApplicationDataPath"/>.
    /// </summary>
    public interface IApplicationPaths
    {
        /// <summary>
        /// Gets the application data path, that is, the ".mfractor" folder where MFractors data (like cached downloads, license information etc) can be placed.
        /// </summary>
        string ApplicationDataPath { get; }

        /// <summary>
        /// Gets the application logs path.
        /// </summary>
        string ApplicationLogsPath { get; }

        /// <summary>
        /// Gets the application temp path.
        /// </summary>
        string ApplicationTempPath { get; }
    }
}
