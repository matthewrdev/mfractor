//using System;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.Linq;
//using AppKit;
//using MFractor.Analytics;
//using MFractor.Code.CodeActions;
//using MFractor.Editor.Tooltips;
//using MFractor.Utilities;
//using MFractor.VS.Mac.Views;
//using MFractor.Work;
//using MFractor.Workspace;
//using MFractor.Workspace.Utilities;
//using Microsoft.VisualStudio.Text.Adornments;
//using Microsoft.VisualStudio.Text.Editor;
//using Microsoft.VisualStudio.Utilities;

//namespace MFractor.VS.Mac.Tooltips
//{
//    [Export(typeof(IViewElementFactory))]
//	[Name("MFractor CodeIssueTooltipModel to UIElement")]
//	[TypeConversion(from: typeof(CodeIssueTooltipModel), to: typeof(NSView))]
//	[Order(After = "default")]
//	sealed class CodeIssueTooltipViewElementFactory : IViewElementFactory
//	{
//		readonly Logging.ILogger log = Logging.Logger.Create();

//		readonly Lazy<IWorkEngine> workEngine;
//        public IWorkEngine  WorkEngine => workEngine.Value;

//		readonly Lazy<IAnalyticsService> analyticsService;
//        public IAnalyticsService AnalyticsService => analyticsService.Value;

//		readonly Lazy<ICodeActionEngine> codeActionEngine;
//		public ICodeActionEngine CodeActionEngine => codeActionEngine.Value;

//		readonly Lazy<IWorkspaceService> workspaceService;
//		public IWorkspaceService WorkspaceService => workspaceService.Value;

//		[ImportingConstructor]
//        public CodeIssueTooltipViewElementFactory(Lazy<IWorkEngine> workEngine,
//										          Lazy<IAnalyticsService> analyticsService,
//                                                  Lazy<ICodeActionEngine> codeActionEngine,
//												  Lazy<IWorkspaceService> workspaceService)
//        {
//            this.workEngine = workEngine;
//            this.analyticsService = analyticsService;
//            this.codeActionEngine = codeActionEngine;
//            this.workspaceService = workspaceService;
//        }

//		public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
//		{
//			// Should never happen if the service's code is correct, but it's good to be paranoid.
//			if (typeof(TView) != typeof(NSView) || !(model is CodeIssueTooltipModel tooltipModel))
//			{
//				throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
//			}

//			var codeIssue = tooltipModel.CodeIssue;
//            if (codeIssue == null)
//            {
//				return default;
//            }

//			var codeActionSuggestions = new List<ICodeActionSuggestion>();

//			var context = tooltipModel.FeatureContext;
//			var location = tooltipModel.InteractionLocation;

//			var actions = CodeActionEngine.RetrieveCodeActions(context, location, CodeActionCategory.Fix);

//			foreach (var action in actions)
//            {
//				try
//				{
//					var suggestions = action.Suggest(context, location);

//					if (suggestions != null && suggestions.Any())
//					{
//						codeActionSuggestions.AddRange(suggestions);
//					}
//				}
//				catch (Exception ex)
//                {
//					log?.Exception(ex);
//                }
//            }

//			CodeIssueView imageTooltip = null;
//			void codeFixClicked(CodeIssueFixSelectedEventArgs args)
//			{
//				try
//				{
//					var action = actions.FirstOrDefault(a => a.Identifier == args.CodeActionSuggestion.CodeActionIdentifier);
//					if (action != null)
//					{
//						if (imageTooltip.Handle != IntPtr.Zero && imageTooltip.Window.Handle != IntPtr.Zero)
//						{
//							imageTooltip.Window.Close();
//						}

//						if (!string.IsNullOrEmpty(action.AnalyticsEvent))
//						{
//							AnalyticsService.Track(action.AnalyticsEvent + " (Tooltip Context)");
//						}

//						var work = action.Execute(context, args.CodeActionSuggestion, location);

//						WorkEngine.ApplyAsync(work);

//						var id = context.Project.GetIdentifier().Guid;
//						(WorkspaceService as WorkspaceService)?.NotifyFileChanged(id, context.Document.FilePath);
//					}
//                    else
//                    {
//						log?.Info("Unable to find the code action to execute for: " + args.CodeActionSuggestion.Description);
//                    }
//				}
//				catch (Exception ex)
//				{
//					log?.Exception(ex);
//				}
//			}

//			imageTooltip = new CodeIssueView(tooltipModel.CodeIssue, codeActionSuggestions, codeFixClicked, tooltipModel.HelpUrl);

//			return imageTooltip as TView;
//		}
//	}
//}