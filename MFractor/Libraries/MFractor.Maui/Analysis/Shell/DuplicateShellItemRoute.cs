using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Shell
{
    class DuplicateShellItemRoute : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string Identifier => "com.mfractor.code.analysis.xaml.duplicate_shellitem_route";

        public override string Name => "Duplicate ShellItem Route";

        public override string Documentation => "Inspects `.ShellItem` elements and validates that their `Route` is unique.";

        public override string DiagnosticId => "MF1047";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.Shell.MetaType))
            {
                return Array.Empty<ICodeIssue>();
            }

            var shellItems = syntax.GetChildren(c =>
            {
                var pageType = context.XamlSemanticModel.GetSymbol(c) as INamedTypeSymbol;
                
                return SymbolHelper.DerivesFrom(pageType, context.Platform.ShellItem.MetaType);
            });

            if (shellItems.Count <= 1)
            {
                return Array.Empty<ICodeIssue>();
            }

            var routes = new Dictionary<string, List<XmlAttribute>>();

            foreach (var item in shellItems)
            {
                var route = item.GetAttributeByName("Route");

                if (route == null || !route.HasValue)
                {
                    continue;
                }

                if (!routes.ContainsKey(route.Value.Value))
                {
                    routes[route.Value.Value] = new List<XmlAttribute>();
                }

                routes[route.Value.Value].Add(route);
            }

            var issues = new List<ICodeIssue>();

            foreach (var route in routes)
            {
                if (route.Value.Count > 1)
                {
                    issues.AddRange(route.Value.Select(r => CreateIssue("The route \"" + route.Key + "\" is also defined on other ShellItem's in this Shell. This may cause unexpected navigation behaviour. Is this intended?", r, r.Span)));
                }
            }

            return issues;
        }
    }
}
