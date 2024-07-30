using System;
using System.Collections.Generic;
using MFractor.Maui.Semantics;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;

namespace MFractor.Maui.ViewModels
{
    public interface IDataBindingGatherer
    {
        IReadOnlyList<BindingExpression> GatherDataBindings(XmlNode node,
                                                            IXamlNamespaceCollection namespaces,
                                                            IXamlSemanticModel xamlSemanticModel,
                                                            IXamlPlatform platform);
    }
}
