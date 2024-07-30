using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.RefreshView;
using MFractor.Work.WorkUnits;
using MFractor.Work;
using MFractor.Xml;
using MFractor.Utilities;

namespace MFractor.Maui.CodeActions.Fix.RefreshView
{
    class LearnMoreAboutRefreshViewContentShouldBeScrollableLayout : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(RefreshViewContentShouldBeScrollableLayout);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_fixes.xaml.learn_more_about_refreshview_content_should_be_scrollable_layout";

        public override string Name => "Learn More About RefreshView Content Should Be Scrollable Layout";

        public override string Documentation => "When using a RefreshView in XAML, the inner content should be a CollectionView, ListView or ScrollView. This code fix opens the documentation that specifies this restriction.";

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Learn more").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            if (context.Platform.Platform != XamlPlatform.XamarinForms)
            {
                return null;
            }

            return new OpenUrlWorkUnit("https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/refreshview", false).AsList();
        }
    }
}
