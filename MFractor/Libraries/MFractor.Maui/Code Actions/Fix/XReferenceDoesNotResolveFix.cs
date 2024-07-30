using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Expressions;
using MFractor.Maui.Syntax;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.CodeActions.Fix
{
    class XReferenceDoesNotResolveFix : FixCodeAction
	{
        public override string Documentation => "Replaces an invalid x:Name reference with a similiarly named x:Name declared in the current document.";

        public override Type TargetCodeAnalyser => typeof(CodeBehindFieldReferenceDoesNotResolveAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.xreference_does_not_exist";

        public override string Name => "Replace x:Name Reference With Autocorrection";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<CodeBehindFieldReferenceFixBundle>();

            return content != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
			 var content = issue.GetAdditionalContent<CodeBehindFieldReferenceFixBundle>();

            return CreateSuggestion("Replace with '" + content.BestMatch + "'").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<CodeBehindFieldReferenceFixBundle>();
			var expression = content.ReferenceExpression;
			var value = "";

            var valueSpan = default(TextSpan);
			if (expression.NameExpression != null)
			{
				value = XamlMarkupExtensionNames.XamarinForms.Name + "=" + content.BestMatch;
                valueSpan = expression.NameExpression.Span;
			}
			else if (expression.Value != null)
			{
				value = content.BestMatch;
                valueSpan = expression.Value.Span;
			}

            return new ReplaceTextWorkUnit(document.FilePath, value, valueSpan).AsList();
        }
	}
}

