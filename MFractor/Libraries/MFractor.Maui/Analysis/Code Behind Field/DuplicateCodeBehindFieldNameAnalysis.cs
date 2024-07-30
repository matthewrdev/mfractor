using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.CodeBehindField
{
    class DuplicateCodeBehindFieldNameAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Checks that the value assigned to an `x:Name` attribute is unique within the scope of the document.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.duplicate_code_behind_field_name";

        public override string Name => "Duplicate Code Behind Field Declarations";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1003";

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, 
                                                            IParsedXamlDocument document, 
                                                            IXamlFeatureContext context)
		{
            if (!syntax.HasValue)
            {
                return null;
            }

            if (!XamlSyntaxHelper.IsCodeBehindFieldName(syntax, context.Namespaces))
			{
                return null;
			}

            // x:Name has a different meaning on VisualStates
            // See: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/visual-state-manager#vsm-markup-on-a-view

            var symbol = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;
            if (SymbolHelper.DerivesFrom(symbol, context.Platform.VisualState.MetaType))
            {
                return null;
            }

            var count = GetCountOfXNameAttributesWithValue(document.GetSyntaxTree().Root, syntax.Value.Value, context.Namespaces);

			if (count <= 1)
            {
                return null;
			}

			var fi = new System.IO.FileInfo(document.FilePath);

			var message = $"The x:Name value '{syntax.Value}' is not unique in {fi.Name}.";

            return CreateIssue(message, syntax, syntax.Span).AsList();
        }

        static int GetCountOfXNameAttributesWithValue(XmlNode node, string value, IXamlNamespaceCollection namespaces)
        {
            if (!node.HasAttributes && node.IsLeaf)
            {
                return 0;
            }

            var count = node.HasAttribute(a =>
            {
                if (!XamlSyntaxHelper.IsCodeBehindFieldName(a, namespaces))
                {
                    return false;
                }

                return a.HasValue && a.Value.Value == value;
            }) ? 1 : 0;

            if (node.HasChildren)
            {
                foreach (var n in node.Children)
                {
                    count += GetCountOfXNameAttributesWithValue(n, value, namespaces);
                }
            }

            return count;
        }
	}
}

