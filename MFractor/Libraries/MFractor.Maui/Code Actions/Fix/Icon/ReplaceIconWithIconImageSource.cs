using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Icon
{
    class ReplaceIconWithIconImageSource : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(ObsoletePropertyUsedAnalysis);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.replace_icon_with_icon_image_source";

        public override string Name => "Replace Icon With IconImageSource";

        public override string Documentation => "Replaces Icon with IconImageSource.";

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return syntax.Name.FullName == "Icon";
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Replace with IconImageSource").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return new ReplaceTextWorkUnit()
            {
                FilePath = document.FilePath,
                Span = syntax.NameSpan,
                Text = "IconImageSource"
            }.AsList();
        }
    }
}