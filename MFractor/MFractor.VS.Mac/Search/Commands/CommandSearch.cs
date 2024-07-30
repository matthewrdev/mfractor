using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Ide;
using MFractor.IOC;
using MFractor.Licensing;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Core.Text;

namespace MFractor.VS.Mac.Search.Commands
{
    class CommandSearch : SearchCategory
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        protected ICommandRepository Commands { get; set; }

        [Import]
        protected ISolutionPad SolutionPad { get; set; }

        [Import]
        protected IAnalyticsService AnalyticsService { get; set; }

        [Import]
        protected ILicensingService LicensingService { get; set; }

        public CommandSearch()
            : base("MFractor")
        {
            Resolver.ComposeParts(this);
        }

        public override void Initialize(MonoDevelop.Components.XwtPopup popupWindow)
        {
            Name = "MFractor";

            base.Initialize(popupWindow);
        }

        public override Task GetResults(ISearchResultCallback searchResultCallback, SearchPopupSearchPattern pattern, System.Threading.CancellationToken token)
        {
            return Task.Run(delegate
            {
                if (!LicensingService.HasActivation)
                {
                    return;
                }

                try
                {
                    var context = DefaultCommandContext.Instance; 

                    var matcher = StringMatcher.GetMatcher(pattern.Pattern, false);

                    foreach (var command in Commands)
                    {
                        if (command is IInternalToolCommand)
                        {
#if !DEBUG
                            continue; // Do not allow internal tools outside of debug mode.
#endif
                        }

                        try
                        {
                            var state = command.GetExecutionState(context);

                            if (state != null && state.CanExecute)
                            {
                                if (matcher.CalcMatchRank(state.Label, out var rank))
                                {
                                    var result = new CommandSearchResult(command, context, AnalyticsService, state, pattern.Pattern, rank);
                                    searchResultCallback.ReportResult(result);
                                }
                            }
                        }
                        catch (NotImplementedException)
                        {
                            // Suppress.
                        }
                        catch (Exception ex)
                        {
                            log?.Warning("An error occured while retrieving the command state for " + command.GetType().ToString() + ". Details: " + ex.ToString());
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

        readonly string[] tags = { "MFractor" };
        public override string[] Tags => tags;

        public override bool IsValidTag(string tag)
        {
            return tags.Contains(tag);
        }
    }
}

