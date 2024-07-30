using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Images;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Images
{
    class ImportMissingImageResource : FixCodeAction
	{
		public override string Documentation => "When you are missing an image resource, MFractor will warn you and give you the option to import it into your Android and iOS projects.";

		public override Type TargetCodeAnalyser => typeof(DetectMissingImageResource);

        public override string Identifier => "com.mfractor.code_fixes.xaml.import_missing_image_resource";

		public override string Name => "Import Missing Image Resource";

		public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

		protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
		{
            return syntax.HasValue;
		}

		protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
		{
            return CreateSuggestion("Import " + syntax.Value + " as a new image resource", 0).AsList();
		}

		protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
		{
            var pendingName = syntax.Value.Value;
			var workspace = context.Workspace;

			var imageBundle = issue.GetAdditionalContent<MissingImageResourceBundle>();

            IReadOnlyList<IWorkUnit> NameConfirmedCallback(string name)
            {
                if (name == pendingName)
				{
                    return null;
				}

                return new ReplaceTextWorkUnit(document.FilePath, name, syntax.Value.Span).AsList();
			}

			return new ImportImageAssetWorkUnit()
			{
                ImageName = pendingName,
				Projects = imageBundle.MissingProjects,
				OnImageNameConfirmedCallback = NameConfirmedCallback,
			}.AsList();
		}
	}
}
