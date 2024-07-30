using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.XamlNamespaces
{
    class XmlnsAssemblyDoesNotResolveAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "Checks that the 'assembly' component of an xmlns statement resolves to an assembly referenced by the project.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier =>"com.mfractor.code.analysis.xaml.xmlns_assembly_does_not_resolve";

        public override string Name => "Unresolved Xmlns Assembly";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1068";

        [Import]
        public IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver { get; set; }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (!syntax.Parent.IsRoot // Can only analyse root elements as namespace decls
                || !syntax.Name.ToString().StartsWith("xmlns", StringComparison.Ordinal) // Can only analyse xmlns statments
                || !syntax.HasValue
                || syntax.Value.Value.StartsWith("http", StringComparison.Ordinal)) // Don't analyse schemas.
            {
                return null;
            }

            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax.Name.LocalName);

            if (xamlNamespace == null || xamlNamespace.AssemblyComponent == null)
            {
                return null;
            }

            var assemblies = XmlnsNamespaceSymbolResolver.GetAssemblies(xamlNamespace, context.Project, context.XmlnsDefinitions);

            if (assemblies != null && assemblies.Any())
            {
                return null;
            }

            if (xamlNamespace.TargetPlatformComponent != null && xamlNamespace.TargetPlatformComponent.HasTargettedPlatform)
            {
                return CreateIssue($"The assembly {xamlNamespace.AssemblyComponent.AssemblyName} could not be resolved within the current solution.", syntax, syntax.Value.Span).AsList();
            }
            else
            {
                return CreateIssue($"The assembly {xamlNamespace.AssemblyComponent.AssemblyName} is not referenced by this project.", syntax, syntax.Value.Span).AsList();
            }
        }
    }
}

