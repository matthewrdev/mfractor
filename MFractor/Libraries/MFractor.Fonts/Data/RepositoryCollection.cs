using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Fonts.Data.Models;
using MFractor.Fonts.Data.Repositories;

namespace MFractor.Fonts.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class RepositoryCollection : IRepositoryCollection
    {
        public void RegisterRepositories(IDatabase database)
        {
            database.RegisterRepository<FontFileAssetRepository, FontFileAsset>(new FontFileAssetRepository());
            database.RegisterRepository<FontGlyphClassBindingRepository, FontGlyphClassBinding>(new FontGlyphClassBindingRepository());
        }
    }
}
