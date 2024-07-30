using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.ImageManager
{
    /// <summary>
    /// The image manager.
    /// <para/>
    /// This part is not exposed via MEF.
    /// </summary>
    public interface IImageManagerController
    {
        bool IsLoading { get; }

        IImageAsset SelectedImageAsset { get; }

        IImageManagerOptions Options { get; }
        IReadOnlyDictionary<string, IImageAsset> ImageAssets { get; }

        System.Threading.Tasks.Task GatherImageAssetsAsync();

        void SetOptions(IImageManagerOptions options);

        void SetSolution(Solution solution, bool force = false);
    }
}
