using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.StaticResources;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.StaticResources
{
    class CorrectStaticResourceNameFix : FixCodeAction
    {
        public override string Documentation => "When a static resource cannot be resolved but there is another closely named resource available to the document, this code fix suggests and replaces the existin resource with that resource.";

        public override Type TargetCodeAnalyser => typeof(UndefinedStaticResourceAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.correct_static_resource_name";

        public override string Name => "Correct Static Resource Name";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (!syntax.HasValue)
            {
                return false;
            }

            var bundle = issue.GetAdditionalContent<UndefinedStaticResourceBundle>();

            return bundle != null && !string.IsNullOrEmpty(bundle.SuggestedStaticResource);
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<UndefinedStaticResourceBundle>();

            return CreateSuggestion("Replace with " + bundle.SuggestedStaticResource).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<UndefinedStaticResourceBundle>();

            var expression = bundle.StaticResourceExpression;

            return new ReplaceTextWorkUnit(context.Document.FilePath, bundle.SuggestedStaticResource, expression.Value.Span).AsList();
        }
    }
}

