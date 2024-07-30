using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Data.Models;
using MFractor.IOC;
using MFractor.Localisation;
using MFractor.Localisation.Data.Models;
using MFractor.Localisation.Tooltips;
using MFractor.VS.Mac.Utilities;
using MFractor.Work;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Ide.CodeCompletion;

namespace MFractor.VS.Mac.Search.Localisation
{
    class ResXLocalisationSearchResult : SearchResult, IAnalyticsFeature
    {
        public string AnalyticsEvent => "ResX Localisation Search";

        public Project Project { get; }
        public ResXLocalisationDefinition ResXLocalisationDefinition { get; }
        public ProjectFile ProjectFile { get; }

        public ResXLocalisationSearchResult(Project project, ResXLocalisationDefinition resXLocalisationDefinition, string pattern, int rank)
            : base(pattern, resXLocalisationDefinition.Key, rank)
        {
            ResXLocalisationDefinition = resXLocalisationDefinition;
            Project = project;
        }

        public override void Activate()
        {
            var workUnitEngine = Resolver.Resolve<IWorkEngine>();
            var navigationService = Resolver.Resolve<ILocalisationNavigationService>();

            workUnitEngine.ApplyAsync(navigationService.Navigate(ResXLocalisationDefinition, Project));

            Resolver.Resolve<IAnalyticsService>().Track(this);
        }

        public override Xwt.Drawing.Image Icon => Xwt.Drawing.Image.FromResource("crosshairs-16.png");

        public override string GetMarkupText(bool selected)
        {
            return ResXLocalisationDefinition.Key;
        }

        public override string GetDescriptionMarkupText()
        {
            return "ResX Localisation key in " + Project.Name;
        }

        public override bool CanActivate => true;

        public double GetWeight(int item)
        {
            return Weight;
        }

        public override Task<TooltipInformation> GetTooltipInformation(CancellationToken token)
        {
            var tooltip = new TooltipInformation();

            tooltip.SignatureMarkup = Resolver.Resolve<ILocalisationTooltipRenderer>().CreateLocalisationTooltip(ResXLocalisationDefinition, Project);

            return Task.FromResult(tooltip.AddMFractorBranding());
        }

        public string GetFileName(int item)
        {
            return ProjectFile.FilePath;
        }
    }
}
