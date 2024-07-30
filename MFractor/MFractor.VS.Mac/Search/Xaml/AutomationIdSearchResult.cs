using System;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Maui.Data.Models;
using MFractor.Maui.Tooltips;
using MFractor.Ide.WorkUnits;
using MFractor.IOC;
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
    class AutomationIdSearchResult : SearchResult, IAnalyticsFeature
    {
        public string AnalyticsEvent => "Automation ID Search";

        public IProjectResourcesDatabase Database { get; }
        public Project Project { get; }
        public AutomationIdDeclaration AutomationIdDeclaration { get; }
        public ProjectFile ProjectFile { get; }

        public AutomationIdSearchResult(IProjectResourcesDatabase database,
                                        Project project,
                                        AutomationIdDeclaration automationIdDeclaration, string pattern, int rank)
            : base(pattern, automationIdDeclaration.Name, rank)
        {
            AutomationIdDeclaration = automationIdDeclaration;
            Database = database;
            Project = project;
            ProjectFile = database.GetRepository<ProjectFileRepository>().Get(automationIdDeclaration.ProjectFileKey);
        }

        public override void Activate()
        {
            var workUnitEngine = Resolver.Resolve<IWorkEngine>();

            var workUnit = new NavigateToFileSpanWorkUnit(AutomationIdDeclaration.Span, ProjectFile.FilePath, Project, true);
            workUnitEngine.ApplyAsync(workUnit);

            Resolver.Resolve<IAnalyticsService>().Track(this);
        }

        public override Xwt.Drawing.Image Icon => Xwt.Drawing.Image.FromResource("crosshairs-16.png");

        public override string GetMarkupText(bool selected)
        {
            return AutomationIdDeclaration.Name + " for " + AutomationIdDeclaration.ParentMetaDataName;
        }

        public override string GetDescriptionMarkupText()
        {
            return "Automation ID in " + Project.Name;
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

            tooltip.SignatureMarkup += Resolver.Resolve<IAutomationIdTooltipRenderer>().CreateTooltip(AutomationIdDeclaration, Project);

            return Task.FromResult(tooltip.AddMFractorBranding());
        }

        public string GetFileName(int item)
        {
            return ProjectFile.FilePath;
        }
    }
}
