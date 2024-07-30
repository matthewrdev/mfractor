using System;
using System.Collections.Generic;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Code
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> that provides the <see cref="IFeatureContextFactory"/>'s within the app domain.
    /// </summary>
    public interface IFeatureContextFactoryRepository : IPartRepository<IFeatureContextFactory>
    {
        /// <summary>
        /// The <see cref="IFeatureContextFactory"/>'s available within the app domain.
        /// </summary>
        IReadOnlyList<IFeatureContextFactory> FeatureContextFactories { get; }

        TFeatureContextFactory GetFeatureContextFactory<TFeatureContextFactory>() where TFeatureContextFactory : class, IFeatureContextFactory;

        IFeatureContextFactory GetInterestedFeatureContextFactory(Project project, string filePath);
    }
}
