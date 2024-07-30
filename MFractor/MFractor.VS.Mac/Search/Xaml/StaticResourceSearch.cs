using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Documentation;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Maui.Data.Repositories;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Core.Text;

namespace MFractor.VS.Mac.Search.Xaml
{
    public class StaticResourceSearch : SearchCategory, IAmDocumented
    {
        private readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        protected IResourcesDatabaseEngine ResourcesDatabaseEngine { get; set; }

        [Import]
        protected IWorkspaceService WorkspaceService { get; set; }

        public StaticResourceSearch()
            : base("Static Resource Search")
        {
            Resolver.ComposeParts(this);
        }

        public override void Initialize(MonoDevelop.Components.XwtPopup popupWindow)
        {
            Name = "Static Resources";
            base.Initialize(popupWindow);
        }

        public override Task GetResults(ISearchResultCallback searchResultCallback, SearchPopupSearchPattern pattern, System.Threading.CancellationToken token)
        {
            return Task.Run(delegate
            {
                if (!Resolver.Resolve<ILicensingService>().IsPaid)
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
                                var repo = db.GetRepository<StaticResourceDefinitionRepository>();
                                var resources = repo.GetAll();

                                foreach (var resource in resources)
                                {
                                    token.ThrowIfCancellationRequested();

                                    if (resource.GCMarked || resource.IsImplicitResource)
                                    {
                                        continue;
                                    }

                                    var rank = 0;
                                    if (matcher.CalcMatchRank(resource.Name, out rank))
                                    {
                                        var result = new StaticResourceSearchResult(db, project, resource, pattern.Pattern, rank);
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

        public string Documentation => "Surfaces all static resource declarations into the global search bar in Visual Studio Mac.";

        public override bool IsValidTag(string tag)
        {
            return tags.Contains(tag);
        }
    }
}

