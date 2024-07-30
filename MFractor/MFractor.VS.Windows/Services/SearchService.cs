using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFractor.Progress;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ISearchProgressService))]
    class SearchProgressService : ISearchProgressService
    {
        public IProgressMonitor StartSearch(string description)
        {
            return new StubProgressMonitor(); // TODO: Implement me.
        }
    }
}
