using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.WorkUnits;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Styles
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IApplyStyleRefactoring))]
    class ApplyStyleRefactoring : IApplyStyleRefactoring
    {
        public IReadOnlyList<IWorkUnit> ApplyStyle(IXamlPlatform platform, XmlNode syntax, IStyle style, string filePath)
        {
            var workUnits = new List<IWorkUnit>();

            var deletions = new List<XmlSyntax>();

            foreach (var property in style.Properties)
            {
                var attribute = syntax.GetAttributeByName(property.Name);

                if (attribute == null || !attribute.HasValue)
                {
                    continue;
                }

                var value = property.Value as ILiteralStylePropertyValue;
                if (value is null)
                {
                    continue;
                }

                if (attribute.Value.Value == value.Value)
                {
                    deletions.Add(attribute);
                }
            }

            workUnits.Add(new DeleteXmlSyntaxWorkUnit()
            {
                FilePath = filePath,
                Syntaxes = deletions,
            });

            // Do we have an existing style attribute?
            var styleSetter = syntax.GetAttributeByName("Style");

            if (styleSetter != null)
            {
                workUnits.Add(new ReplaceTextWorkUnit()
                {
                    FilePath = filePath,
                    Text = "Style=\"{" + platform.StaticResourceExtension.MarkupExpressionName + " " + style.Name + "}\"",
                    Span = styleSetter.Span,
                });
            }
            else
            {
                workUnits.Add(new ReplaceTextWorkUnit()
                {
                    FilePath = filePath,
                    Text = " Style=\"{" + platform.StaticResourceExtension.MarkupExpressionName + " " + style.Name + "}\"",
                    Span = TextSpan.FromBounds(syntax.NameSpan.End, syntax.NameSpan.End),
                });
            }

            return workUnits;
        }

        public bool CanApplyStyle(XmlNode syntax, IStyle style)
        {
            if (style.IsImplicit)
            {
                return false;
            }

            // How many properties does 
            var overloadCount = 0;

            foreach (var property in style.Properties)
            {
                var attribute = syntax.GetAttributeByName(property.Name);

                // The property applied by the style does not exist on the childe element
                if (attribute == null || !attribute.HasValue)
                {
                    return false;
                }

                // The styles setter value does not exist.
                var value = property.Value as ILiteralStylePropertyValue;
                if (value is null)
                {
                    return false;
                }

                // The values do not match, this is considered an overloaded style value.
                if (attribute.Value.Value != value.Value)
                {
                    overloadCount++;
                }
            }

            return overloadCount < style.Properties.Count / 2; // 2 == heuristic to detect that it is likely that this style does not fit the shape of what the user is trying to achieve.
        }
    }
}
