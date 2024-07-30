using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Code
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFeatureContextFactoryRepository))]
    class FeatureContextFactoryRepository : PartRepository<IFeatureContextFactory>, IFeatureContextFactoryRepository
    {
        [ImportingConstructor]
        public FeatureContextFactoryRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IFeatureContextFactory> FeatureContextFactories => Parts;

        public TFeatureContextFactory GetFeatureContextFactory<TFeatureContextFactory>() where TFeatureContextFactory : class, IFeatureContextFactory
        {
            return FeatureContextFactories.OfType<TFeatureContextFactory>().FirstOrDefault();
        }

        public IFeatureContextFactory GetInterestedFeatureContextFactory(Project project, string filePath)
        {
            return FeatureContextFactories.FirstOrDefault(fcf => fcf.IsInterestedInDocument(project, filePath));
        }
    }
}