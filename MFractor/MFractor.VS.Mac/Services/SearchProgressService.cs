using System;
using System.ComponentModel.Composition;
using MFractor.Progress;
using MFractor.VS.Mac.Progress;
using MonoDevelop.Ide;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ISearchProgressService))]
    class SearchProgressService : ISearchProgressService
    {
        public IProgressMonitor StartSearch(string description)
        {
            var searchMonitor = IdeApp.Workbench.ProgressMonitors.GetSearchProgressMonitor(true);

            return new IdeProgressMonitor(description, searchMonitor);
        }
    }
}