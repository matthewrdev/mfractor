using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;

namespace MFractor.Code.Scaffolding
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IScaffolderRepository))]
    class ScaffolderRepository : PartRepository<IScaffolder>, IScaffolderRepository
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public ScaffolderRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
            indexedScaffolders = new Lazy<IReadOnlyDictionary<string, IScaffolder>>(() =>
           {
               var result = new Dictionary<string, IScaffolder>();

               foreach (var scaffolder in Scaffolders)
               {
                   try
                   {
                       result.Add(scaffolder.Identifier, scaffolder);
                   }
                   catch (Exception ex)
                   {
                       log?.Warning("The scaffolder " + scaffolder.GetType() + " failed to be indexed. Reason: " + ex);
                   }
               }

               return result;
           });
        }

        readonly Lazy<IReadOnlyDictionary<string, IScaffolder>> indexedScaffolders;
        public IReadOnlyDictionary<string, IScaffolder> IndexedScaffolders => indexedScaffolders.Value;

        public IReadOnlyList<IScaffolder> Scaffolders => Parts;

        public IEnumerable<IScaffolder> GetAvailableScaffolders(IScaffoldingContext ScaffoldingContext)
        {
            return Scaffolders.Where(c => c.IsAvailable(ScaffoldingContext));
        }

        public IScaffolder GetScaffolder(string id)
        {
            if (!IndexedScaffolders.ContainsKey(id))
            {
                return default;
            }

            return IndexedScaffolders[id];
        }
    }
}