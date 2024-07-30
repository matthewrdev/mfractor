using System;
using System.Collections.Generic;
using MFractor.Ide.WorkUnits;
using MFractor.Text;
using MFractor.VS.Windows.WorkUnitHandlers.Navigation;
using Microsoft.VisualStudio.Shell.TableManager;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class NavigationTableDataSource : ITableDataSource, IDisposable
    {
        public string SourceTypeIdentifier => StandardTableDataSources.AnyDataSource;

        public string Identifier => "bcc63099-d9ff-4452-812f-c18a458fa996";

        public string DisplayName => "MFractor Search Results";

        ITableDataSink sink;
        readonly ILineCollectionFactory lineCollectionFactory;

        public bool IsDisposed { get; private set; } = false;

        public NavigationTableDataSource(ITableManagerProvider tableManagerProvider,
                                         ILineCollectionFactory lineCollectionFactory)
        {
            this.lineCollectionFactory = lineCollectionFactory;

            var tableManager = tableManagerProvider.GetTableManager(StandardTables.TasksTable);
            tableManager.AddSource(this, new string[]
            {
                StandardTableKeyNames.BuildTool,
                StandardTableKeyNames.Text,
                StandardTableKeyNames.DocumentName,
                StandardTableKeyNames.ProjectName,
                StandardTableKeyNames.Line,
                StandardTableKeyNames.Column
            });
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            this.sink = sink;
            return this;
        }

        internal void Clear()
        {
            sink?.RemoveAllEntries();
        }

        internal void Add(IReadOnlyList<NavigateToFileSpanWorkUnit> locations)
        {
            sink?.AddSnapshot(new NavigationTableEntriesSnapshot(locations, lineCollectionFactory));
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
