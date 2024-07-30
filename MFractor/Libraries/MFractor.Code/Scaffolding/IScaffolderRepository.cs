using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Code.Scaffolding
{
    public interface IScaffolderRepository : IPartRepository<IScaffolder>
    {
        IReadOnlyList<IScaffolder> Scaffolders { get; }

        IEnumerable<IScaffolder> GetAvailableScaffolders(IScaffoldingContext ScaffoldingContext);

        IScaffolder GetScaffolder(string id);
    }
}
