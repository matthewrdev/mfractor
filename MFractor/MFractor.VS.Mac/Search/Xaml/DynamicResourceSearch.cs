using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Documentation;
using MFractor.IOC;
using MFractor.Maui;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Core.Text;

namespace MFractor.VS.Mac.Search.Xaml
{
    public class DynamicResourceSearch : SearchCategory, IAmDocumented
    {
        private readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        protected IResourcesDatabaseEngine ResourcesDatabaseEngine { get; set; }

        [Import]
        protected IWorkspaceService WorkspaceService { get; set; }

        [Import]
        protected IDynamicResourceResolver DynamicResourceResolver { get; set; }

        public DynamicResourceSearch()
            : base("Dynamic Resource Search")
        {
            Resolver.ComposeParts(this);
        }

        public string Documentation => "Surfaces Dynamic Resource declarations into the global search bar in Visual Studio Mac.";

        public override void Initialize(MonoDevelop.Components.XwtPopup popupWindow)
        {
            Name = "Dynamic Resources";
            base.Initialize(popupWindow);
        }

        public override Task GetResults(ISearchResultCallback searchResultCallback, SearchPopupSearchPattern pattern, System.Threading.CancellationToken token)
        {
            return Task.Run(async delegate
            {
                try
                {
                    var workspace = WorkspaceService.CurrentWorkspace;

                    if (workspace != null && workspace.CurrentSolution != null)
                    {
                        var matcher = StringMatcher.GetMatcher(pattern.Pattern, false);

                        foreach (var project in workspace.CurrentSolution.Projects)
                        {
                            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
                            var resources = DynamicResourceResolver.GetAvailableDynamicResources(project);
                            token.ThrowIfCancellationRequested();

                            foreach (var resource in resources.DistinctBy(r => r.Definition.Name))
                            {
                                token.ThrowIfCancellationRequested();

                                var rank = 0;
                                if (matcher.CalcMatchRank(resource.Definition.Name, out rank))
                                {
                                    var result = new DynamicResourceSearchResult(database, resource.Project, resource.Definition, pattern.Pattern, rank);
                                    searchResultCallback.ReportResult(result);
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
