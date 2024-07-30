using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Semantics;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;

namespace MFractor.Maui.ViewModels
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDataBindingGatherer))]
    class DataBindingGatherer : IDataBindingGatherer
    {
        public IReadOnlyList<BindingExpression> GatherDataBindings(XmlNode node, IXamlNamespaceCollection namespaces, IXamlSemanticModel xamlSemanticModel, IXamlPlatform platform)
        {
            var xmlns = namespaces.ResolveNamespace(node);

            if (xmlns != null)
            {
                if (XamlSchemaHelper.IsPlatformSchema(xmlns, platform)
                    && node.Name.LocalName == platform.DataTemplate.Name)
                {
                    return null;
                }
            }

            var expressions = new List<BindingExpression>();

            IEnumerable<BindingExpression> bindings = null;
            if (node.HasAttributes)
            {
                bindings = node.Attributes.Select(attr =>  xamlSemanticModel.GetExpression(attr) as BindingExpression)
                                                .OfType<BindingExpression>();
                if (bindings != null && bindings.Any())
                {
                    expressions.AddRange(bindings);
                }
            }

            if (node.HasChildren)
            {
                foreach (var n in node.Children)
                {
                    bindings = GatherDataBindings(n, namespaces, xamlSemanticModel, platform);

                    if (bindings != null && bindings.Any())
                    {
                        expressions.AddRange(bindings);
                    }
                }
            }

            return expressions;
        }
    }
}