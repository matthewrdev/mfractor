using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using MFractor.Ide.WorkUnits;
using MFractor.Progress;
using MFractor.Text;
using MFractor.VS.Windows.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.TableManager;
using Task = System.Threading.Tasks.Task;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class NavigateToFileSpansWorkUnitHandler : WorkUnitHandler<NavigateToFileSpansWorkUnit>
    {
        private DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        readonly Lazy<ITableManagerProvider> tableManagerProvider;
        ITableManagerProvider TableManagerProvider => tableManagerProvider.Value;

        readonly Lazy<IDispatcher> dispatcher;
        IDispatcher Dispatcher => dispatcher.Value;

        readonly Lazy<ILineCollectionFactory> lineCollectionFactory;
        ILineCollectionFactory LineCollectionFactory => lineCollectionFactory.Value;

        [ImportingConstructor]
        public NavigateToFileSpansWorkUnitHandler(Lazy<ITableManagerProvider> tableManagerProvider,
                                                  Lazy<IDispatcher> dispatcher,
                                                  Lazy<ILineCollectionFactory> lineCollectionFactory)
        {
            this.tableManagerProvider = tableManagerProvider;
            this.dispatcher = dispatcher;
            this.lineCollectionFactory = lineCollectionFactory;
        }

        NavigationTableDataSource dataSource;

        public override Task<IWorkExecutionResult> OnExecute(NavigateToFileSpansWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            if (workUnit.AutoOpenSingleResults && workUnit.Locations.Count == 1)
            {
                var result = new WorkExecutionResult();
                result.AddPostProcessedWorkUnits(workUnit.Locations);

                return Task.FromResult<IWorkExecutionResult>(result);
            }

            Dispatcher.InvokeOnMainThreadAsync(() =>
           {
               if (dataSource is null || dataSource.IsDisposed)
               {
                   dataSource = new NavigationTableDataSource(TableManagerProvider, LineCollectionFactory);
               }

               dataSource.Clear();

               dataSource.Add(workUnit.Locations);

               DTE.ExecuteCommand("View.TaskList");
           }).ConfigureAwait(false);

            return Task.FromResult<IWorkExecutionResult>(default);
        }
    }
}
