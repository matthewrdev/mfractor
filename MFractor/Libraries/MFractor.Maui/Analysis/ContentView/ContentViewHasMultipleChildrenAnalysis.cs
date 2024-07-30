using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class ContentViewHasMultipleChildrenAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects usages of the `ContentView` element and checks that it only has a single child view. Assigning multiple children into a `ContentView` is a common mistake where the developer usually intended to wrap the chid views with a `Grid` or a `StackLayout`.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.content_view_has_multiple_children";

        public override string Name => "ContentView Has Multiple Direct Children";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1006";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (typeSymbol == null)
            {
                return null;
            }

            if (!SymbolHelper.DerivesFrom(typeSymbol, context.Platform.ContentView.MetaType))
            {
                return null;
            }

            if (!TryResolveDoesOverloadContent(typeSymbol, context.Platform, out var overloadsContent)
                || overloadsContent)
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

            return CreateIssue($"{syntax.Name} has multiple child views. A ContentView should only have 1 direct child element.", syntax, syntax.NameSpan).AsList();
        }

        bool TryResolveDoesOverloadContent(INamedTypeSymbol typeSymbol, IXamlPlatform platform, out bool overloadsContent)
        {
            overloadsContent = false;

            var type = typeSymbol;
            while (type != null)
            {
                var attributes = type.GetAttributes();

                if (!attributes.Any())
                {
                    type = type.BaseType;
                    continue;
                }

                var contentProperty = attributes.FirstOrDefault(a => SymbolHelper.DerivesFrom(a.AttributeClass, platform.ContentPropertyAttribute.MetaType));

                if (contentProperty != null && contentProperty.ConstructorArguments.Any())
                {
                    var content = contentProperty.ConstructorArguments.First().Value as string;

                    if (string.IsNullOrEmpty(content))
                    {
                        overloadsContent = false;
                        return true;
                    }

                    var property = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(type, content);

                    if (property == null)
                    {
                        overloadsContent = false;
                        return true;
                    }

                    if (SymbolHelper.DerivesFrom(property.Type, "System.Collections.IEnumerable")
                        || SymbolHelper.DerivesFrom(property.Type, "System.Collections.IEnumerable`1"))
                    {
                        overloadsContent = true;
                    }

                    return true;
                }

                type = type.BaseType;
            }

            return true;
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

