using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Workspace;

namespace MFractor.Images.Optimisation
{
    /// <summary>
    /// The image optimisation service can be used to significantly reduce the size of images.
    /// <para/>
    /// It uses TinyPNG to apply image shrinking, reducing image assets by up to 70% with no loss to visual quality.
    /// </summary>
    public interface IImageOptimisationService
    {
        /// <summary>
        /// Is an API key for TinyPNG present?
        /// </summary>
        /// <value><c>true</c> if has API key; otherwise, <c>false</c>.</value>
        bool HasApiKey { get; }

        /// <summary>
        /// Sets the API key for TinyPNG.
        /// </summary>
        /// <param name="apiKey">API key.</param>
        void SetApiKey(string apiKey);

        /// <summary>
        /// Optimise the image at the <paramref name="filePath"/>.
        /// </summary>
        /// <returns>The optimise.</returns>
        /// <param name="filePath">File path.</param>
        /// <param name="progressMessageCallback">Progress message callback.</param>
        /// <param name="cancellation">Cancellation.</param>
        OptimisationResult Optimise(string filePath, Action<string> progressMessageCallback, CancellationToken cancellation);

        /// <summary>
        /// Optimise the image at the <paramref name="filePath"/>, asynchronously.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="filePath">File path.</param>
        /// <param name="progressMessageCallback">Progress message callback.</param>
        /// <param name="cancellation">Cancellation.</param>
        Task<OptimisationResult> OptimiseAsync(string filePath, Action<string> progressMessageCallback, CancellationToken cancellation);

        /// <summary>
        /// Optimise the <paramref name="projectFile"/>.
        /// </summary>
        /// <returns>The optimise.</returns>
        /// <param name="projectFile">Project file.</param>
        /// <param name="progressMessageCallback">Progress message callback.</param>
        /// <param name="cancellation">Cancellation.</param>
        OptimisationResult Optimise(IProjectFile projectFile, Action<string> progressMessageCallback, CancellationToken cancellation);

        /// <summary>
        /// Optimise the <paramref name="projectFile"/>, asynchronously.
        /// </summary>
        /// <returns>The optimised image.</returns>
        /// <param name="projectFile">Project file.</param>
        /// <param name="progressMessageCallback">Progress message callback.</param>
        /// <param name="cancellation">Cancellation.</param>
        Task<OptimisationResult> OptimiseAsync(IProjectFile projectFile, Action<string> progressMessageCallback, CancellationToken cancellation);

        IReadOnlyList<OptimisationResult> Optimise(IEnumerable<IProjectFile> projectFiles, Action<string> progressMessageCallback, CancellationToken cancellation);
        Task<IReadOnlyList<OptimisationResult>> OptimiseAsync(IEnumerable<IProjectFile> projectFiles, Action<string> progressMessageCallback, CancellationToken cancellation);

        IReadOnlyList<OptimisationResult> Optimise(IImageAsset imageAsset, Action<string> progressMessageCallback, CancellationToken cancellation);
        Task<IReadOnlyList<OptimisationResult>> OptimiseAsync(IImageAsset imageAsset, Action<string> progressMessageCallback, CancellationToken cancellation);

        IReadOnlyList<OptimisationResult> Optimise(IEnumerable<IImageAsset> imageAssets, Action<string> progressMessageCallback, CancellationToken cancellation);
        Task<IReadOnlyList<OptimisationResult>> OptimiseAsync(IEnumerable<IImageAsset> imageAssets, Action<string> progressMessageCallback, CancellationToken cancellation);
    }
}
