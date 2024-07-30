using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Maui.Navigation;
using MFractor.Navigation;

namespace MFractor.Editor.Tooltips
{
    [Obsolete("The TextContentTooltipModel and should be replaced with a ClassifiedTextElement instead.")]
    public class TextContentTooltipModel
    {
        public string Content { get; set; }

        public string HelpUrl { get; set; }

        public bool ShowFooter { get; set; } = true;

        public INavigationContext NavigationContext { get; set; }

        public INavigationSuggestion NavigationSuggestion { get; set; }

        public IFeatureContext FeatureContext { get; set; }

        public IReadOnlyList<ICodeAction> CodeActions { get; set; }

        public InteractionLocation InteractionLocation { get; set; }
    }
}
