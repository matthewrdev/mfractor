using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Maui.Data.Repositories;
using MFractor.IOC;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Core.Text;

namespace MFractor.VS.Mac.Search.Xaml
{
    public class AutomationIdSearch : SearchCategory
    {
        private readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        protected IResourcesDatabaseEngine ResourcesDatabaseEngine { get; set; }

        [Import]
        protected IWorkspaceService WorkspaceService { get; set; }

        public AutomationIdSearch()
            : base("Automation ID Search")
        {
            Resolver.ComposeParts(this);
        }

        public override void Initialize(MonoDevelop.Components.XwtPopup popupWindow)
        {
            Name = "Automation ID";
            base.Initialize(popupWindow);
        }

        public override Task GetResults(ISearchResultCallback searchResultCallback, SearchPopupSearchPattern pattern, System.Threading.CancellationToken token)
        {
            return Task.Run(delegate
            {
                if (!Resolver.Resolve<MFractor.Licensing.ILicensingService>().IsPaid)
                {
                    return;
                }

                try
                {
                    var workspace = WorkspaceService.CurrentWorkspace;

                    if (workspace != null && workspace.CurrentSolution != null)
                    {
                        var matcher = StringMatcher.GetMatcher(pattern.Pattern, false);

                        foreach (var project in workspace.CurrentSolution.Projects)
                        {
                            token.ThrowIfCancellationRequested();
                            var db = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

                            if (db != null && db.IsValid)
                            {
                                var repo = db.GetRepository<AutomationIdDeclarationRepository>();
                                var resources = repo.GetAll();

                                foreach (var automationId in resources)
                                {
                                    token.ThrowIfCancellationRequested();

                                    if (automationId.GCMarked)
                                    {
                                        continue;
                                    }

                                    int rank = 0;
                                    if (matcher.CalcMatchRank(automationId.Name, out rank))
                                    {
                                        var result = new AutomationIdSearchResult(db, project, automationId, pattern.Pattern, rank);
                                        searchResultCallback.ReportResult(result);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }, token);
        }

        readonly string[] tags = { "xaml" };
        public override string[] Tags => tags;

        public override bool IsValidTag(string tag)
        {
            return tags.Contains(tag);
        }
    }
}

