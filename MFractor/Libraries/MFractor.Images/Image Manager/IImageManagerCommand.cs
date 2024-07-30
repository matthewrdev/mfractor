using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;

namespace MFractor.Images.ImageManager
{
    /// <summary>
    /// An <see cref="ICommand"/> that executes within the context of MFractor's image asset manager.
    /// <para/>
    /// Implementations of the <see cref="IImageManagerCommand"/> interface are automatically detected and added to the <see cref="IImageManagerCommandRepository"/>.
    /// </summary>
    [InheritedExport]
    public interface IImageManagerCommand : ICommand, IAnalyticsFeature
    {
    }
}
