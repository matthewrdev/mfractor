using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Ide.WorkUnits;
using MFractor.Progress;
using MFractor.Text;
using MFractor.Work;
using MonoDevelop.Ide;
using MonoDevelop.Ide.FindInFiles;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class NavigateToFileSpansWorkUnitHandler : WorkUnitHandler<NavigateToFileSpansWorkUnit>
    {
        readonly Lazy<IWorkEngine> workUnitEngine;
        IWorkEngine WorkUnitEngine => workUnitEngine.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        readonly Lazy<ILineCollectionFactory> lineCollectionFactory;
        public ILineCollectionFactory LineCollectionFactory => lineCollectionFactory.Value;


        [ImportingConstructor]
        public NavigateToFileSpansWorkUnitHandler(Lazy<IWorkEngine> workUnitEngine,
                                                  Lazy<IDispatcher> dispatcher,
                                                  Lazy<ILineCollectionFactory> lineCollectionFactory)
        {
            this.workUnitEngine = workUnitEngine;
            this.dispatcher = dispatcher;
            this.lineCollectionFactory = lineCollectionFactory;
        }

        public override async Task<IWorkExecutionResult> OnExecute(NavigateToFileSpansWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            if (workUnit.Locations == null)
            {
                return null;
            }

            var locations = workUnit.Locations.Where(s => s != null).ToList();
            if (!locations.Any())
            {
                return null;
            }

            await Dispatcher.InvokeOnMainThreadAsync(() =>
            {
                if (locations.Count > 1 || !workUnit.AutoOpenSingleResults)
                {
                    using (var search = IdeApp.Workbench.ProgressMonitors.GetSearchProgressMonitor(true, true))
                    {
                        if (progressMonitor.CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        var files = new Dictionary<string, ILineCollection>();

                        foreach (var location in locations)
                        {
                            search.ReportResult(CreateSearchResult(location, files));
                        }
                    }
                }
                else
                {
                    WorkUnitEngine.ApplyAsync(locations.First());
                }
            });

            return null;
        }

        SearchResult CreateSearchResult(NavigateToFileSpanWorkUnit workUnit, Dictionary<string, ILineCollection> files)
        {
            if (!files.ContainsKey(workUnit.FilePath))
            {
                files[workUnit.FilePath] = LineCollectionFactory.Create(new FileInfo(workUnit.FilePath));
            }

            var lines = files[workUnit.FilePath];
            var line = lines.GetLineAtOffset(workUnit.Span.Start);

            return SearchResult.Create(workUnit.FilePath, workUnit.Span.Start, workUnit.Span.Length, line.Content, line.Index, workUnit.Span.Start - line.Span.Start);
        }
    }
}
