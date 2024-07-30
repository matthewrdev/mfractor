using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.XamlNamespaces;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    class XmlNamespaceDoesNotExistsFix : FixCodeAction
	{
        public override string Documentation => "Replaces an xml namespace prefix with the auto-corrected xml namespace.";

        public override Type TargetCodeAnalyser => typeof(XmlNamespaceDoesNotExistAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.xml_namespace_does_not_exist";

        public override string Name => "Replace XML Namespace With Autocorrection";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var name = issue.GetAdditionalContent<string>();
            return !string.IsNullOrEmpty(name) && syntax.Name?.Namespace != name;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var newName = issue.GetAdditionalContent<string>();

            return CreateSuggestion($"Replace with '{newName}'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue,XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var newName = issue.GetAdditionalContent<string>();

            newName += ":" + syntax.Name.LocalName;

            var workUnits = new List<IWorkUnit>
            {
                new ReplaceTextWorkUnit(document.FilePath, newName, syntax.NameSpan)
            };

            if (!syntax.IsSelfClosing)
			{
                workUnits.Add(new ReplaceTextWorkUnit(document.FilePath, "</"+ newName + ">", syntax.ClosingTagSpan));
			}

			return workUnits;
		}
	}
}

