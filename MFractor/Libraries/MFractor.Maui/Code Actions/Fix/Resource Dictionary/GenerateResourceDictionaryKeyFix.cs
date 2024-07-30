using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.ResourceDictionary;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.CodeActions.Fix.ResourceDictionary
{
    class GenerateResourceDictionaryKeyFix : FixCodeAction
	{
        public override string Documentation => "When a Xaml node that is declared within a `ResourceDictionary` is missing the `x:Key` attribute, this fix will automatically create a new `x:Key` attribute for the node.";

        public override Type TargetCodeAnalyser => typeof(MissingResourceDictionaryKey);

        public override string Identifier => "com.mfractor.code_fixes.xaml.missing_resource_key";

        public override string Name => "Generate Resource Dictionary Key";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Generate a key for this resource", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var microsoftNamespace = context.Namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);

			var key = GenerateNonClashingResourceKey(syntax);

			var content = $" {microsoftNamespace.Prefix}:{Keywords.MicrosoftSchema.DictionaryKey}=\"{key}\"";

            return new ReplaceTextWorkUnit(document.FilePath, content, TextSpan.FromBounds(syntax.NameSpan.End, syntax.NameSpan.End)).AsList();
        }

        string GenerateNonClashingResourceKey(XmlNode syntax)
		{
			var parent = syntax.Parent;

			var baseName = StringExtensions.FirstCharToLower(syntax.Name.LocalName);
			var key = baseName;
			var clashes = syntax.HasAttribute(attr => attr.Name.LocalName == key);

			var iter = 1;
			while (clashes)
			{
				key = baseName + iter;
				clashes = syntax.HasAttribute(attr => attr.Name.LocalName == key);
			}

			return key;
		}
}
}
