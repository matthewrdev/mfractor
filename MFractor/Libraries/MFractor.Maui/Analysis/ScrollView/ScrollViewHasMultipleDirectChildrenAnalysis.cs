using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class ScrollViewHasMultipleDirectChildrenAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects usages of ScrollView and checks that it only has a single child view. Assigning multiple children to a ScrollView is a common mistake where the developer usually intended to wrap the chid views with a Grid or a StackLayout.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.scroll_view_has_multiple_children";

        public override string Name => "ScrollView Has Multiple Direct Children";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1099";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (typeSymbol == null)
            {
                return null;
            }

            if (!SymbolHelper.DerivesFrom(typeSymbol, context.Platform.ScrollView.MetaType))
            {
                return null;
            }

            if (!syntax.HasChildren)
            {
                return null;
            }

            var childrenValid = HasValidChildren(syntax);
            if (childrenValid)
            {
                return null;
            }

            return CreateIssue($"{syntax.Name} has multiple child views. A ScrollView should only have 1 direct child element.", syntax, syntax.NameSpan).AsList();
        }

        bool HasValidChildren(XmlNode syntax)
        {
            var contentViewSetterName = syntax.Name.FullName + ".Content";
            var contentSetter = syntax.GetChildNode(contentViewSetterName);

            if (contentSetter != null)
            {
                return contentSetter.HasChildren ? contentSetter.Children.Count <= 1 : true;
            }

            var innerViewCount = 0;
            if (syntax.HasChildren)
            {
                for (var i = 0; i < syntax.Children.Count; ++i)
                {
                    var child = syntax.Children[i];
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

