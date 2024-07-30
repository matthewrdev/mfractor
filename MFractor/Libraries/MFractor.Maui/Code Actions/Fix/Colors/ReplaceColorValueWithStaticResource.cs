using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Colors;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using MFractor.Xml;
using MFractor.Work;

namespace MFractor.Maui.CodeActions.Fix.Colors
{
    class ReplaceColorValueWithStaticResource : FixCodeAction
    {
        public override Type TargetCodeAnalyser => default;

        public override IEnumerable<Type> TargetCodeAnalysers { get; } = new Type[] { typeof(ColorValueCanBeReplacedWithStaticResource), typeof(ColorValueCloselyMatchesAvailableStaticResource) };

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.replace_color_value_with_static_resource";

        public override string Name => "Replace Color Value With StaticResource";

        public override string Documentation => "When a color value matches a static resource color, this code fix replaces the hex value with the named color";

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ColorValueCanBeReplacedWithStaticResourceBundle>();

            return bundle != null && bundle.MatchingColorDefinitions.Any();
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ColorValueCanBeReplacedWithStaticResourceBundle>();

            var suggestions = new List<ICodeActionSuggestion>();

            for (var i = 0; i < bundle.MatchingColorDefinitions.Count; ++i)
            {
                var color = bundle.MatchingColorDefinitions[i];
                suggestions.Add(CreateSuggestion($"Replace with {color.Name} ({color.Color.GetHexString(true)})", i));
            }

            return suggestions;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ColorValueCanBeReplacedWithStaticResourceBundle>();

            var match = bundle.MatchingColorDefinitions[suggestion.ActionId];

            return new ReplaceTextWorkUnit(document.FilePath, "{" + context.Platform.StaticResourceExtension.MarkupExpressionName + " " + match.Name + "}", syntax.Value.Span).AsList();
        }

        protected ICodeIssue GetCodeIssue(XmlAttribute attr, int position)
        {
            var issues = attr.Get(MFractor.MetaDataKeys.Analysis.Issues, default(List<ICodeIssue>));

            return FindCandidateIssues(issues, position).FirstOrDefault();
        }
    }
}
