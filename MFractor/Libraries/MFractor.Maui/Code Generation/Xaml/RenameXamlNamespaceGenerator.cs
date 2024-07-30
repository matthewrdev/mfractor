using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Documents;
using MFractor.Maui.Semantics;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Xaml
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IRenameXamlNamespaceGenerator))]
    class RenameXamlNamespaceGenerator : XamlCodeGenerator, IRenameXamlNamespaceGenerator
    {
        public override string Documentation => "Generates a series of replace text fixes to rename a namespace";

        public override string Identifier => "com.mfractor.code_gen.xaml.xaml.rename_xaml_namespace";

        public override string Name => "Rename Xaml Namespace Generator";

        public List<IWorkUnit> RenameNamespace(string currentNamespace, string newNamespace, IParsedXamlDocument context, IXamlSemanticModel semanticModel, bool shouldRenameNamespaceDeclaration)
        {
            var workUnit = ApplyRenameNode(context.GetSyntaxTree().Root, currentNamespace, newNamespace, context, semanticModel, shouldRenameNamespaceDeclaration);

            return workUnit.Cast<IWorkUnit>().ToList();
        }

        List<ReplaceTextWorkUnit> ApplyRenameNode(XmlNode node, string currentNamespace, string newNamespace, IParsedXamlDocument context, IXamlSemanticModel semanticModel, bool shouldRenameNamespaceDeclaration)
        {
            var workUnits = new List<ReplaceTextWorkUnit>();

            if (node.HasChildren)
            {
                foreach (var n in node.Children)
                {
                    var nodeFixes = ApplyRenameNode(n, currentNamespace, newNamespace, context, semanticModel, shouldRenameNamespaceDeclaration);

                    if (nodeFixes != null && nodeFixes.Any())
                    {
                        workUnits.AddRange(nodeFixes);
                    }
                }
            }

            if (node.HasAttributes)
            {
                foreach (var attr in node.Attributes)
                {
                    var attrFixes = ApplyRenameAttribute(attr, currentNamespace, newNamespace, context, semanticModel, shouldRenameNamespaceDeclaration);

                    if (attrFixes != null && attrFixes.Any())
                    {
                        workUnits.AddRange(attrFixes);
                    }
                }
            }

            if ((node.Name.HasNamespace && node.Name.Namespace == currentNamespace)
                || (!node.Name.HasNamespace && string.IsNullOrEmpty(currentNamespace)))
            {
                workUnits.Add(new ReplaceTextWorkUnit()
                {
                    Text = CreateNamespaceUsage(newNamespace, node.Name.LocalName),
                    FilePath = context.FilePath,
                    Span = node.NameSpan,
                });

                if (!node.IsSelfClosing)
                {
                    workUnits.Add(new ReplaceTextWorkUnit()
                    {
                        Text = CreateNamespaceUsage(newNamespace, node.Name.LocalName),
                        FilePath = context.FilePath,
                        Span = node.ClosingTagNameSpan,
                    });
                }
            }

            return workUnits;
        }

        List<ReplaceTextWorkUnit> ApplyRenameAttribute(XmlAttribute attribute, string currentNamespace, string newNamespace, IParsedXamlDocument context, IXamlSemanticModel semanticModel, bool shouldRenameNamespaceDeclaration)
        {
            var workUnits = new List<ReplaceTextWorkUnit>();

            if (shouldRenameNamespaceDeclaration)
            {
                if (attribute.Name.HasNamespace
                    && attribute.Name.Namespace == "xmlns"
                    && attribute.Name.LocalName == currentNamespace)
                {
                    workUnits.Add(new ReplaceTextWorkUnit()
                    {
                        Text = $"xmlns:{newNamespace}",
                        FilePath = context.FilePath,
                        Span = attribute.NameSpan,
                    });
                }
                else if ((attribute.Name.HasNamespace && attribute.Name.Namespace == currentNamespace)
                  || (!attribute.Name.HasNamespace && string.IsNullOrEmpty(currentNamespace)))
                {
                    workUnits.Add(new ReplaceTextWorkUnit()
                    {
                        Text = CreateNamespaceUsage(newNamespace, attribute.Name.LocalName),
                        FilePath = context.FilePath,
                        Span = attribute.NameSpan,
                    });
                }
            }

            var expression = semanticModel.GetExpression(attribute);
            if (expression != null)
            {
                var expressionFixes = ApplyRenameExpression(expression, currentNamespace, newNamespace, context);

                if (expressionFixes != null && expressionFixes.Any())
                {
                    workUnits.AddRange(expressionFixes);
                }
            }

            return workUnits;
        }

        List<ReplaceTextWorkUnit> ApplyRenameExpression(Expression expression, string currentNamespace, string newNamespace, IParsedXamlDocument context)
        {
            var workUnits = new List<ReplaceTextWorkUnit>();

            if (expression is DotNetTypeSymbolExpression)
            {
                var symbol = expression as DotNetTypeSymbolExpression;
                if (string.IsNullOrEmpty(currentNamespace) && symbol.HasNamespace == false)
                {
                    workUnits.Add(new ReplaceTextWorkUnit()
                    {
                        Text = newNamespace,
                        FilePath = context.FilePath,
                        Span = symbol.NamespaceSpan,
                    });
                }
                else if (symbol.HasNamespace && symbol.Namespace == currentNamespace)
                {
                    workUnits.Add(new ReplaceTextWorkUnit()
                    {
                        Text = newNamespace,
                        FilePath = context.FilePath,
                        Span = symbol.NamespaceSpan,
                    });
                }
            }

            var innerExpressions = expression.Children;

            foreach (var child in expression.Children)
            {
                var expressionFixes = ApplyRenameExpression(child, currentNamespace, newNamespace, context);
                if (expressionFixes != null && expressionFixes.Any())
                {
                    workUnits.AddRange(expressionFixes);
                }
            }

            return workUnits;
        }


        protected string CreateNamespaceUsage(string namespaceName, string elementName)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return elementName;
            }

            return $"{namespaceName}:{elementName}";
        }
    }
}
