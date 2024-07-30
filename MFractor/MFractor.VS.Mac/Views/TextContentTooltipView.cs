using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.Analytics;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Editor.Tooltips;
using MFractor.IOC;
using MFractor.Navigation;
using MFractor.Work;

namespace MFractor.VS.Mac.Views
{
    public class TextContentTooltipView : NSStackView
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        NSTextField summaryLabel;

        NSBrandedFooter footer;

        public string Content => TooltipModel?.Content;
        public INavigationSuggestion NavigationSuggestion => TooltipModel?.NavigationSuggestion;
        public INavigationContext NavigationContext => TooltipModel?.NavigationContext;
        public InteractionLocation InteractionLocation => TooltipModel?.InteractionLocation;
        public IFeatureContext FeatureContext => TooltipModel?.FeatureContext;
        public IReadOnlyList<ICodeAction> CodeActions => TooltipModel?.CodeActions;
        public string HelpUrl => TooltipModel?.HelpUrl;

        public TextContentTooltipModel TooltipModel { get; private set; }

        public TextContentTooltipView() : base()
        {
            Initialise(TooltipModel);
        }

        public TextContentTooltipView(IntPtr handle) : base(handle)
        {
            Initialise(TooltipModel);
        }

        [Export("initWithFrame:")]
        public TextContentTooltipView(CGRect frameRect) : base(frameRect)
        {
            Initialise(TooltipModel);
        }

        public TextContentTooltipView(TextContentTooltipModel tooltipModel)
            : base()
        {
            Initialise(tooltipModel);
        }

        public void Initialise(TextContentTooltipModel tooltipModel)
        {
            try
            {
                TooltipModel = tooltipModel;

                this.ClearChildren();

                Orientation = NSUserInterfaceLayoutOrientation.Vertical;
                Alignment = NSLayoutAttribute.Left;

                BuildTextContent();

                BuildNavigationLabel();

                BuildCodeActionSuggestions();

                BuildFooter();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void BuildTextContent()
        {
            if (!string.IsNullOrEmpty(Content))
            {
                summaryLabel = new NSTextField()
                {
                    Editable = false,
                    Selectable = false,
                    Bezeled = false,
                    DrawsBackground = false,
                    LineBreakMode = NSLineBreakMode.ByWordWrapping,
                    Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
                    StringValue = Content ?? string.Empty,
                };
                InsertView(summaryLabel, (uint)Views.Length, NSStackViewGravity.Top);
            }
        }

        void BuildFooter()
        {
            InsertView(new NSBox()
            {
                BoxType = NSBoxType.NSBoxSeparator,

            }, (uint)Views.Length, NSStackViewGravity.Top);

            footer = new NSBrandedFooter(HelpUrl);

            InsertView(footer, (uint)Views.Length, NSStackViewGravity.Top);
        }

        void BuildCodeActionSuggestions()
        {
            if (FeatureContext != null && CodeActions != null)
            {
                foreach (var codeAction in CodeActions)
                {
                    var suggestions = codeAction.Suggest(FeatureContext, InteractionLocation);

                    if (suggestions != null && suggestions.Any())
                    {
                        foreach (var suggestion in suggestions)
                        {
                            var actionButton = AppKitHelper.CreateLinkButton(suggestion.Description, async () =>
                            {
                                var result = codeAction.Execute(FeatureContext, suggestion, InteractionLocation);

                                if (result != null && result.Any())
                                {
                                    Resolver.Resolve<IAnalyticsService>().Track(codeAction);
                                    await Resolver.Resolve<IWorkEngine>().ApplyAsync(result);
                                }
                            });

                            InsertView(actionButton, (uint)Views.Length, NSStackViewGravity.Top);
                        }
                    }
                }
            }
        }

        private void BuildNavigationLabel()
        {
            if (NavigationSuggestion != null
                && NavigationContext != null
                && !string.IsNullOrEmpty(NavigationSuggestion.Label))
            {
                var navigationButton = AppKitHelper.CreateLinkButton(NavigationSuggestion.Label, async () =>
                {
                    var result = await Resolver.Resolve<INavigationService>().Navigate(NavigationContext, NavigationSuggestion);

                    if (result != null && result.Any())
                    {
                        Resolver.Resolve<IAnalyticsService>().Track(NavigationSuggestion.Label);
                        await Resolver.Resolve<IWorkEngine>().ApplyAsync(result);
                    }
                });

                InsertView(navigationButton, (uint)Views.Length, NSStackViewGravity.Top);
            }
        }
    }
}
