using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.ContentPage
{
    class ContentPageHasMultipleChildrenAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects usages of `ContentPage` and checks that it only has a single child view. Assigning multiple children into a `ContentPage` is a common mistake where the developer usually intended to wrap the chid views with a `Grid` or a `StackLayout`.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.content_page_has_multiple_children";

        public override string Name => "ContentPage Has Multiple Direct Children";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1005";

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (typeSymbol == null)
            {
                return null;
            }

            if (!SymbolHelper.DerivesFrom(typeSymbol, context.Platform.ContentPage.MetaType))
            {
                return null;
            }

            if (!syntax.HasChildren)
            {
                return null;
            }

            var childrenValid = ContentViewHasValidChildren(syntax);

            if (childrenValid)
            {
                return null;
            }

            return CreateIssue($"{syntax.Name} has multiple child views. A ContentPage should only have 1 direct child element.", syntax, syntax.NameSpan).AsList();
        }

        bool ContentViewHasValidChildren(XmlNode element)
        {
            var contentViewSetterName = element.Name.FullName + ".Content";
            var contentSetter = element.GetChildNode(contentViewSetterName);

            if (contentSetter != null)
            {
                return contentSetter.HasChildren ? contentSetter.Children.Count <= 1 : true;
            }

            var innerViewCount = 0;
            if (element.HasChildren)
            {
                for (var i = 0; i < element.Children.Count; ++i)
                {
                    var child = element.Children[i];
                    if (!XamlSyntaxHelper.IsPropertySetter(child))
                    {
                        innerViewCount++;
                        if (innerViewCount > 1)
                        {
                            break;
                        }
                    }
                }
            }

            return innerViewCount <= 1;
        }
    }
}
