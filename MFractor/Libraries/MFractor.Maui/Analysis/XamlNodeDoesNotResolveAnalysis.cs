using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Utilities.SymbolVisitors;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Analysis
{
    class XamlNodeDoesNotResolveAnalysis : XamlCodeAnalyser
    {
        [Import]
        public IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver { get; set; }

        public override string Documentation => "Checks that xaml nodes map to a valid .NET symbol.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.xaml_node_does_not_resolve";

        public override string Name => "Xaml Node Resolves";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1075";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.PropertySetterNodeExecutionFilter;

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax);
            if (symbol != null)
            {
                return null;
            }

            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax);

            if (XamlSchemaHelper.IsSchema(xamlNamespace, XamlSchemas.MicrosoftSchemaUrl))
            {
                return null;
            }

            if (XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                return null;
            }

            var clrNamespace = xamlNamespace.ClrNamespaceComponent.Namespace;

            var span = syntax.NameSpan;
            var elementName = syntax.Name.LocalName;

            if (XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                string propertyName;
                XamlSyntaxHelper.ExplodePropertySetter(syntax, out elementName, out propertyName);
                span = TextSpan.FromBounds(span.Start, span.End - (propertyName.Length + 1));
            }

            var assemblies = XmlnsNamespaceSymbolResolver.GetAssemblies(xamlNamespace, context.Project, context.XmlnsDefinitions);
            var namespaces = XmlnsNamespaceSymbolResolver.GetNamespaces(xamlNamespace, context.Project, context.XmlnsDefinitions)?.ToList();
            if (assemblies is null || !assemblies.Any() || namespaces is null || !namespaces.Any())
            {
                return null;
            }

            // Locate the namespace and assembly, perform a fuzzy search for a similiarly named symbol.
            var similiarSymbol = FormsSymbolHelper.ResolveNearlyNamedTypeFromAssemblies(assemblies, elementName, context.Compilation);
            if (similiarSymbol != null)
            {
                return null;
            }

            // Locate the namespace and assembly, perform a fuzzy search for a similiarly named symbol.
            var importableSymbols = FindImportableTypes(context.Project, elementName);

            var message = $"The namespace '{clrNamespace}' does not contain a type named '{elementName}'.";

            if (similiarSymbol != null 
                && similiarSymbol.ContainingNamespace.ToString() == clrNamespace)
            {
                message += $"\n\nDid you mean '{similiarSymbol.Name}'?";
            } 
            else if (importableSymbols != null && importableSymbols.Any())
            {
                if (importableSymbols.Count() == 1)
                {
                    var type = importableSymbols.FirstOrDefault();
                    message += $"\n\nWould you like to import '{type.Name}' from '{type.ContainingNamespace.ToString()}'?";
                }
                else
                {
                    message += $"\n\nWould you like to import '{elementName}' from one of the {importableSymbols.Count()} available assemblies that contain this type?";
                }

                return CreateIssue(message, syntax, span, importableSymbols.ToList()).AsList();
            }

            return CreateIssue(message, syntax, span, similiarSymbol).AsList();
        }

        public static IEnumerable<INamedTypeSymbol> FindImportableTypes(Project project,
                                                                        string currentTypeName)
        {
            var visitor = new FindTypeByNameVisitor(currentTypeName);

            if (project.TryGetCompilation(out var compilation))
            {
                visitor.VisitAssembly(compilation.Assembly);

                foreach (var mdr in project.MetadataReferences)
                {
                    if (compilation.GetAssemblyOrModuleSymbol(mdr) is IAssemblySymbol assembly)
                    {
                        visitor.Visit(assembly);
                    }
                }
            }

            foreach (var pr in project.ProjectReferences)
            {
                var reference = project.Solution.GetProject(pr.ProjectId);

                if (reference != null && reference.TryGetCompilation(out var temp))
                {
                    visitor.VisitAssembly(temp.Assembly);
                }
            }

            return visitor.Matches;
        }

    }
}

