using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Documentation;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Localisation.Data.Repositories;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Core.Text;

namespace MFractor.VS.Mac.Search.Localisation
{
    public class ResXLocalisationSearch : SearchCategory, IAmDocumented
    {
        private readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        protected IResourcesDatabaseEngine ResourcesDatabaseEngine { get; set; }

        [Import]
        protected IWorkspaceService WorkspaceService { get; set; }

        public ResXLocalisationSearch()
            : base("ResX Localisation Search")
        {
            Resolver.ComposeParts(this);
        }

        public override void Initialize(MonoDevelop.Components.XwtPopup popupWindow)
        {
            Name = "ResX Localisations";

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
                                var repo = db.GetRepository<ResXLocalisationDefinitionRepository>();
                                var resources = repo.GetAll();

                                foreach (var definition in resources)
                                {
                                    token.ThrowIfCancellationRequested();

                                    if (definition.GCMarked)
                                    {
                                        continue;
                                    }

                                    if (matcher.CalcMatchRank(definition.Key, out var rank))
                                    {
                                        var result = new ResXLocalisationSearchResult(project, definition, pattern.Pattern, rank);
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

        public string Documentation => "Surfaces all AutomationIDs that are declaraed in XAML into the global search bar in Visual Studio Mac.";

        public override bool IsValidTag(string tag)
        {
            return tags.Contains(tag);
        }
    }
}

