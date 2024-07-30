using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.RefreshView
{
    class RefreshViewContentShouldBeScrollableLayout : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects the inner element of a refresh view and verifies that it derives from either a ListView, ScrollView or CollectionView.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.refresh_view_content_must_be_layout";

        public override string Name => "RefreshView Content Should Be Scrollable Layout";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1093";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (typeSymbol == null)
            {
                return null;
            }

            if (!SymbolHelper.DerivesFrom(typeSymbol, context.Platform.RefreshView.MetaType))
            {
                return null;
            }

            if (!syntax.HasChildren)
            {
                return null;
            }

            var content = GetContent(syntax);
            if (content is null)
            {
                return null;
            }

            var contentSymbol = context.XamlSemanticModel.GetSymbol(content) as ITypeSymbol;

            if (contentSymbol is null)
            {
                return null;
            }

            if (SymbolHelper.DerivesFrom(contentSymbol, context.Platform.ScrollView.MetaType)
                || SymbolHelper.DerivesFrom(contentSymbol, context.Platform.CollectionView.MetaType)
                || SymbolHelper.DerivesFrom(contentSymbol, context.Platform.ListView.MetaType))
            {
                return null;
            }

            return CreateIssue($"A RefreshView's content should be a scrollable layout such as a CollectionView, ListView or ScrollView.\n\nUsing {contentSymbol.Name} inside the RefreshView may result in undefined behaviour.", syntax, syntax.NameSpan).AsList();
        }

        XmlNode GetContent(XmlNode syntax)
        {
            var contentViewSetterName = syntax.Name.FullName + ".Content";
            var contentSetter = syntax.GetChildNode(contentViewSetterName);

            if (contentSetter != null)
            {
                return contentSetter.HasChildren ? contentSetter.Children.FirstOrDefault() : null;
            }

            if (syntax.HasChildren)
            {
                for (var i = 0; i < syntax.Children.Count; ++i)
                {
                    var child = syntax.Children[i];
                    if (!XamlSyntaxHelper.IsPropertySetter(child))
                    {
                        return child;
                    }
                }
            }

            return null;
        }
    }
}

