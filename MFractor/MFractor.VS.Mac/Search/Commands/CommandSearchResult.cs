using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Commands;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Ide.CodeCompletion;

namespace MFractor.VS.Mac.Search.Commands
{
    public class CommandSearchResult : SearchResult
    {
        readonly ICommand command;
        readonly ICommandContext commandContext;
        readonly IAnalyticsService analyticsService;
        readonly ICommandState commandState;

        public CommandSearchResult(ICommand command,
                                   ICommandContext commandContext,
                                   IAnalyticsService analyticsService,
                                   ICommandState commandState,
                                   string pattern,
                                   int rank)
            : base(pattern, commandState.Label, rank)
        {
            this.command = command;
            this.commandContext = commandContext;
            this.analyticsService = analyticsService;
            this.commandState = commandState;
        }
        public override void Activate()
        {
            command.Execute(commandContext);

            if (command is IAnalyticsFeature analyticsFeature)
            {
                analyticsService.Track(analyticsFeature);
            }
        }

        public override string GetMarkupText(bool selected)
        {
            return commandState.Label;
        }

        public override string GetDescriptionMarkupText()
        {
            return commandState.Description;
        }

        public override bool CanActivate => true;

        public override Task<TooltipInformation> GetTooltipInformation(CancellationToken token)
        {
            var tooltipInfo = new TooltipInformation()
            {
                SignatureMarkup = commandState.Label,
            };

            if (!string.IsNullOrEmpty(commandState.Description))
            {
                tooltipInfo.AddCategory("Description", commandState.Description);
            }

            return Task.FromResult(tooltipInfo);
        }

        public string GetFileName(int item)
        {
            return string.Empty;
        }
    }
}
