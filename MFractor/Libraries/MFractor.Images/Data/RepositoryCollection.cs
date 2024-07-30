using System;
using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Images.Data.Models;
using MFractor.Images.Data.Repositories;

namespace MFractor.Images.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class RepositoryCollection : IRepositoryCollection
    {
        public void RegisterRepositories(IDatabase database)
        {
            database.RegisterRepository<ImageAssetFileRepository, ImageAssetFile>(new ImageAssetFileRepository());
            database.RegisterRepository<ImageAssetDefinitionRepository, ImageAssetDefinition>(new ImageAssetDefinitionRepository());
        }
    }
}
