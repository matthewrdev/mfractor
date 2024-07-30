using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Symbols;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.CodeActions.Fix.UnresolvedXamlNode
{
    class ImportUnresolvedReferenceFix : FixCodeAction
	{
        public override string Documentation => "This fix action replaces a mispelt xaml node that can be resolved in an assembly with its correct .NET symbol name.";

        public override Type TargetCodeAnalyser => typeof(Analysis.XamlNodeDoesNotResolveAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.autocorrect_unresolved_reference";

        public override string Name => "Replace Node With Auto-Correction";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        readonly Lazy<IXamlTypeResolver> xamlTypeResolver;
        public IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

        readonly Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver;
        public IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver => xmlnsNamespaceSymbolResolver.Value;

        [ImportingConstructor]
        public ImportUnresolvedReferenceFix(Lazy<IXamlTypeResolver> xamlTypeResolver,
                                            Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver)
        {
            this.xamlTypeResolver = xamlTypeResolver;
            this.xmlnsNamespaceSymbolResolver = xmlnsNamespaceSymbolResolver;
        }

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<INamedTypeSymbol>();
            if (symbol == null)
            {
                return false;
            }

            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax);

            var symbols = XmlnsNamespaceSymbolResolver.GetNamespaces(xamlNamespace, context.Project, context.XmlnsDefinitions);

            foreach (var s in symbols)
            {
                var fullType = s + "." + symbol.Name;

                if (fullType == symbol.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<INamedTypeSymbol>();

            return CreateSuggestion($"Replace with '{symbol.Name}'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<INamedTypeSymbol>();

            var beginSpan = syntax.NameSpan;
            var endSpan = default(TextSpan);

			if (!syntax.IsSelfClosing)
			{
                endSpan = syntax.ClosingTagSpan;
			}

            if (XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                XamlSyntaxHelper.ExplodePropertySetter(syntax, out _, out var propertyName);
                beginSpan = TextSpan.FromBounds(beginSpan.Start, beginSpan.End - (propertyName.Length + 1));
                if (!syntax.IsSelfClosing)
                {
                    endSpan = TextSpan.FromBounds(endSpan.Start, endSpan.End - (propertyName.Length + 1));
                }
            }

            var newName = symbol.Name;
            if (!string.IsNullOrEmpty(syntax.Name.Namespace))
			{
				newName = syntax.Name.Namespace + ":" + symbol.Name;
			}

            var workUnits = new List<IWorkUnit>
            {
                new ReplaceTextWorkUnit(document.FilePath, newName, beginSpan)
            };

            if (!syntax.IsSelfClosing)
			{
				workUnits.Add(new ReplaceTextWorkUnit(document.FilePath, "</" + newName + ">", endSpan));
			}

			return workUnits;
		}
	}
}

