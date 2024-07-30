using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Ide.WorkUnits;
using MFractor.IOC;
using MFractor.Maui;
using MFractor.Maui.Data.Models;
using MFractor.Maui.Tooltips;
using MFractor.VS.Mac.Utilities;
using MFractor.Work;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Models;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Ide.CodeCompletion;

namespace MFractor.VS.Mac.Search.Xaml
{
    class DynamicResourceSearchResult : SearchResult, IAnalyticsFeature
    {
        public string AnalyticsEvent => "Dynamic Resource Search";

        public IProjectResourcesDatabase Database { get; }
        public Project Project { get; }
        public ProjectFile ProjectFile { get; }
        public DynamicResourceDefinition DynamicResource { get; }

        public DynamicResourceSearchResult(IProjectResourcesDatabase database,
                                        Project project,
                                        DynamicResourceDefinition dynamicResource, string pattern, int rank)
            : base(pattern, dynamicResource.Name, rank)
        {
            DynamicResource = dynamicResource;
            Database = database;
            Project = project;
            ProjectFile = Database.GetRepository<ProjectFileRepository>().Get(DynamicResource.PrimaryKey);
        }

        public override void Activate()
        {
            if (!Resolver.Resolve<MFractor.Licensing.ILicensingService>().IsPaid)
            {
                return;
            }

            var workUnitEngine = Resolver.Resolve<IWorkEngine>();

            var declarations = Resolver.Resolve<IDynamicResourceResolver>().FindNamedDynamicResources(Project, DynamicResource.Name);

            var result = new List<NavigateToFileSpanWorkUnit>();

            var projectFileRepo = Database.GetRepository<ProjectFileRepository>();

            foreach (var d in declarations)
            {
                var pf = projectFileRepo.Get(d.Definition.ProjectFileKey);

                if (pf != null)
                {
                    result.Add(new NavigateToFileSpanWorkUnit(d.Definition.ExpressionSpan, pf.FilePath, Project));
                }
            }

            workUnitEngine.ApplyAsync(new NavigateToFileSpansWorkUnit(result)).ConfigureAwait(false);

            Resolver.Resolve<IAnalyticsService>().Track(this);
        }

        public override Xwt.Drawing.Image Icon => Xwt.Drawing.Image.FromResource("crosshairs-16.png");

        public override string GetMarkupText(bool selected)
        {
            return DynamicResource.Name + " in " + Project.Name;
        }

        public override string GetDescriptionMarkupText()
        {
            return "DynamicResource in " + Project.Name;
        }

        public override bool CanActivate => true;

        public double GetWeight(int item)
        {
            Console.WriteLine(this.PlainText + " " + Weight);
            return Weight;
        }

        public override Task<TooltipInformation> GetTooltipInformation(CancellationToken token)
        {
            var tooltip = new TooltipInformation();

            tooltip.SignatureMarkup += Resolver.Resolve<IDynamicResourceTooltipRenderer>().CreateTooltip(DynamicResource.Name, Project);

            return Task.FromResult(tooltip.AddMFractorBranding());
        }

        public string GetFileName(int item)
        {
            return ProjectFile.FilePath;
        }
    }
}
