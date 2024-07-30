using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using MFractor.Analytics;
using MFractor.Code.CodeActions;
using MFractor.Editor.Tooltips;
using MFractor.VS.Windows.Views;
using MFractor.Work;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Windows.Tooltips.CodeIssue
{
    [Export(typeof(IViewElementFactory))]
	[Name("MFractor CodeIssueTooltipModel to FrameworkElement")]
	[TypeConversion(from: typeof(CodeIssueTooltipModel), to: typeof(UIElement))]
	[Order(Before = "default")]
	sealed class CodeIssueTooltipViewElementFactory : IViewElementFactory
	{
		readonly Logging.ILogger log = Logging.Logger.Create();

		readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

		readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

		readonly Lazy<ICodeActionEngine> codeActionEngine;
        public ICodeActionEngine CodeActionEngine => codeActionEngine.Value;

        [ImportingConstructor]
        public CodeIssueTooltipViewElementFactory(Lazy<IWorkEngine> workEngine,
										          Lazy<IAnalyticsService> analyticsService,
                                                  Lazy<ICodeActionEngine> codeActionEngine)
        {
            this.workEngine = workEngine;
            this.analyticsService = analyticsService;
            this.codeActionEngine = codeActionEngine;
        }

		public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
		{
			// Should never happen if the service's code is correct, but it's good to be paranoid.
			if (typeof(TView) != typeof(UIElement) || !(model is CodeIssueTooltipModel tooltipModel))
			{
				throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
			}

			var codeIssue = tooltipModel.CodeIssue;
            if (codeIssue == null)
            {
				return default;
            }

			var codeActionSuggestions = new List<ICodeActionSuggestion>();

			var context = tooltipModel.FeatureContext;
			var location = tooltipModel.InteractionLocation;

			var actions = CodeActionEngine.RetrieveCodeActions(context, location, CodeActionCategory.Fix);

			foreach (var action in actions)
            {
                var suggestions = action.Suggest(context, location);

                codeActionSuggestions.AddRange(suggestions);
            }
            void codeFixClicked(CodeIssueFixSelectedEventArgs args)
            {
                try
                {
                    var wpfTextView = textView as IWpfTextView;

                    var action = actions.FirstOrDefault(a => a.Identifier == args.CodeActionSuggestion.CodeActionIdentifier);
                    if (action != null)
                    {
                        if (!string.IsNullOrEmpty(action.AnalyticsEvent))
                        {
                            AnalyticsService.Track(action.AnalyticsEvent + " (Tooltip Context)");
                        }

                        var workUnits = action.Execute(context, args.CodeActionSuggestion, location);

                        if (workUnits != null && workUnits.Any())
                        {
                            var results = workUnits.ToList();

                            WorkEngine.ApplyAsync(results).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            var view = new CodeIssueView(tooltipModel.CodeIssue, codeActionSuggestions, codeFixClicked, tooltipModel.HelpUrl);
            
            return view as TView;
        }
    }
}