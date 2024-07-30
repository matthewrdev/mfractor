using System;
using MFractor.Utilities;
using MFractor.Workspace;

namespace MFractor.Images.Optimisation
{
    /// <summary>
    /// The result of a image optimisation  
    /// </summary>
    public struct OptimisationResult
    {
        /// <summary>
        /// The absolute file path to the file that was optmised.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; }

        /// <summary>
        /// The project file that was optmised. This may be null depending if <see cref="IImageOptimisationService.Optimise(string, Action{string}, System.Threading.CancellationToken)"/>  or <see cref="IImageOptimisationService.OptimiseAsync(string, Action{string}, System.Threading.CancellationToken)"/> was used.
        /// </summary>
        /// <value>The project file.</value>
        public IProjectFile ProjectFile { get; }

        /// <summary>
        /// Does this <see cref="OptimisationResult"/> have a project file>
        /// </summary>
        /// <value><c>true</c> if has project file; otherwise, <c>false</c>.</value>
        public bool HasProjectFile => ProjectFile != null;

        /// <summary>
        /// Was the optimisation successfull?
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success { get; }

        /// <summary>
        /// What was the original file size of the image in bytes prior to optimisation?
        /// </summary>
        /// <value>The size before in bytes.</value>
        public long SizeBeforeInBytes { get; }

        /// <summary>
        /// What is the new file size of the image in bytes after optimisation?
        /// </summary>
        /// <value>The size after in bytes.</value>
        public long SizeAfterInBytes { get; }

        /// <summary>
        /// What is the difference between the <see cref="SizeBeforeInBytes"/> and <see cref="SizeAfterInBytes"/>?
        /// <para/>
        /// This represents the total savings in bytes after optimisation.
        /// </summary>
        /// <value>The difference bytes.</value>
        public long DifferenceBytes => SizeBeforeInBytes - SizeAfterInBytes;

        /// <summary>
        /// If this optimisation was not successful, the 
        /// </summary>
        /// <value>The failure message.</value>
        public string FailureMessage { get; }

        public string BeforeSize => FileSizeHelper.GetFormattedFileSize(SizeBeforeInBytes);

        public string AfterSize => FileSizeHelper.GetFormattedFileSize(SizeAfterInBytes);

        public string DifferenceSize => FileSizeHelper.GetFormattedFileSize(DifferenceBytes);

        public string DifferenceSummary => String.Format("{0:0.#}", SavedPercentage) + "%/" + DifferenceSize + " smaller";

        public double Percentage => ((double)SizeAfterInBytes / (double)SizeBeforeInBytes) * 100.0;

        public double SavedPercentage => 100.0 - Percentage;

        public string Summary => FilePath + "\nFrom " + BeforeSize + " to " + AfterSize + " (" + DifferenceSummary +")";

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Images.Optimisation.OptimisationResult"/> struct.
        /// </summary>
        /// <param name="projectFile">Project file.</param>
        /// <param name="success">If set to <c>true</c> success.</param>
        /// <param name="sizeBeforeInBytes">Size before in bytes.</param>s
        /// <param name="sizeAfterInBytes">Size after in bytes.</param>
        /// <param name="failureMessage">Failure message.</param>
        public OptimisationResult(IProjectFile projectFile,
                                  bool success,
                                  long sizeBeforeInBytes,
                                  long sizeAfterInBytes,
                                  string failureMessage = "")
        {
            FilePath = projectFile?.FilePath;
            ProjectFile = projectFile;
            Success = success;
            SizeBeforeInBytes = sizeBeforeInBytes;
            SizeAfterInBytes = sizeAfterInBytes;
            FailureMessage = failureMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Images.Optimisation.OptimisationResult"/> struct.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="success">If set to <c>true</c> success.</param>
        /// <param name="sizeBeforeInBytes">Size before in bytes.</param>
        /// <param name="sizeAfterInBytes">Size after in bytes.</param>
        /// <param name="failureMessage">Failure message.</param>
        public OptimisationResult(string filePath,
                                  bool success,
                                  long sizeBeforeInBytes,
                                  long sizeAfterInBytes,
                                  string failureMessage = "")
        {
            FilePath = filePath;
            ProjectFile = null;
            Success = success;
            SizeBeforeInBytes = sizeBeforeInBytes;
            SizeAfterInBytes = sizeAfterInBytes;
            FailureMessage = failureMessage;
        }

        public static OptimisationResult CreateFailure(string filePath, string message)
        {
            return new OptimisationResult(filePath, false, 0, 0, message);
        }

        public static OptimisationResult CreateFailure(IProjectFile projectFile)
        {
            return CreateFailure(projectFile, string.Empty);
        }

        public static OptimisationResult CreateFailure(IProjectFile projectFile, string message)
        {
            return new OptimisationResult(projectFile, false, 0, 0, message);
        }

        public static OptimisationResult CreateFailure(IProjectFile projectFile, Exception ex)
        {
            return CreateFailure(projectFile, ex.Message);
        }

        public static OptimisationResult CreateFailure(string filePath, Exception ex)
        {
            return new OptimisationResult(filePath, false, 0, 0, ex.Message);
        }
    }
}
