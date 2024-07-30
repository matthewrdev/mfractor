using System;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Ide.WorkUnits;
using MFractor.IOC;
using MFractor.Maui.Data.Models;
using MFractor.Maui.Tooltips;
using MFractor.VS.Mac.Utilities;
using MFractor.Work;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Models;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Ide.CodeCompletion;

namespace MFractor.VS.Mac.Search.Xaml
{
    class StaticResourceSearchResult : SearchResult, IAnalyticsFeature
    {
        public string AnalyticsEvent => "XAML Resource Search";

        public IProjectResourcesDatabase Database { get; }
        public Project Project { get; }
        public StaticResourceDefinition StaticResourceDefinition { get; }
        public ProjectFile ProjectFile { get; }

        public StaticResourceSearchResult(IProjectResourcesDatabase database, 
                                        Project project, 
                                        StaticResourceDefinition staticResourceDefinition, 
                                        string pattern, 
                                        int rank)
            : base(pattern, staticResourceDefinition.Name, rank)
        {
            StaticResourceDefinition = staticResourceDefinition;
            Database = database;
            Project = project;
            ProjectFile = database.GetRepository<ProjectFileRepository>().Get(staticResourceDefinition.ProjectFileKey);
        }

        public override void Activate()
        {
            var workUnitEngine = Resolver.Resolve<IWorkEngine>();

            var workUnit = new NavigateToFileSpanWorkUnit(TextSpan.FromBounds(StaticResourceDefinition.NameStart, StaticResourceDefinition.NameEnd), ProjectFile.FilePath, Project, true);
            workUnitEngine.ApplyAsync(workUnit);

            Resolver.Resolve<IAnalyticsService>().Track(this);
        }

        public override Xwt.Drawing.Image Icon => Xwt.Drawing.Image.FromResource("feather-box-16.png");

        public override string GetMarkupText(bool selected)
        {
            return StaticResourceDefinition.Name + " - " + StaticResourceDefinition.SymbolMetaType;
        }

        public override string GetDescriptionMarkupText()
        {
            return "Static Resource in " + Project.Name;
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

            tooltip.SignatureMarkup += Resolver.Resolve<IStaticResourceTooltipRenderer>().CreateTooltip(StaticResourceDefinition, Project);

            return Task.FromResult(tooltip.AddMFractorBranding());
        }

        public string GetFileName(int item)
        {
            return ProjectFile.FilePath;
        }
    }

}
