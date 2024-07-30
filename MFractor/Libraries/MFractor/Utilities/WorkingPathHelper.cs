using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities
{
    /// <summary>
    /// A collection of helper methods for discovering MFractors working path for a file, directory or solution.
    /// </summary>
    public static class WorkingPathHelper
    {
        /// <summary>
        /// The name of the MFractors working folder.
        /// </summary>
        public const string WorkingFolderName = ".mfractor";

        /// <summary>
        /// Gets MFractors working path for the provided <paramref name="fileInfo"/>.
        /// </summary>
        public static string GetMFractorWorkingPath(FileInfo fileInfo)
        {
            return GetMFractorWorkingPath(fileInfo.Directory);
        }

        /// <summary>
        /// Gets MFractors working path for the provided <paramref name="directoryInfo"/>.
        /// </summary>
        public static string GetMFractorWorkingPath(DirectoryInfo directoryInfo)
        {
            var workingPath = Path.Combine(directoryInfo.FullName, WorkingFolderName);

            return workingPath;
        }

        /// <summary>
        /// Gets MFractors working path for the provided <paramref name="solution"/>.
        /// </summary>
        public static string GetMFractorWorkingPath(Solution solution)
        {
            var fileInfo = new FileInfo(solution.FilePath);

            return GetMFractorWorkingPath(fileInfo);
        }
    }
}
