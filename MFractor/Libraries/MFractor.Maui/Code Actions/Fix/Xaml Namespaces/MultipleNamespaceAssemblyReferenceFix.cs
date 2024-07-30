using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.XamlNamespaces;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Fix.XamlNamespaces
{
    class MultipleNamespaceAssemblyReferenceFix : FixCodeAction
	{
        public override string Documentation => "When multiple namespaces reference the same .NET namespace and assembly, this code fix will remove duplicates by replace all occurances of a particular namespace another namespace name to make sure only one XML namespace references an assembly and namespace.";

        public override Type TargetCodeAnalyser => typeof(DuplicateNamespaceAssemblyReferenceAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.multiple_namespace_assembly_references";

        public override string Name => "Rename Duplicate Namespace References";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public IRenameXamlNamespaceGenerator RenameNamespaceGenerator { get; set; }

        bool IsNamespace(XmlAttribute attribute)
        {
            return attribute.Name.HasNamespace ? attribute.Name.Namespace == "xmlns" : attribute.Name.LocalName == "xmlns";
        }

        protected List<XmlAttribute> RetrieveRelatedAttributesToRename(XmlAttribute attribute)
        {
            var namespaces = new List<XmlAttribute>();

            if (attribute.Parent.HasAttributes)
            {
                foreach (var ns in attribute.Parent.Attributes)
                {
                    if (ns.Value.Value == attribute.Value.Value
                        && IsNamespace(attribute))
                    {
                        namespaces.Add(ns);
                    }
                }
            }

            namespaces = namespaces.OrderBy(attr => attr.Name.FullName).ToList();

            return namespaces;
        }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Remove duplicate namespaces").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var namespaces = RetrieveRelatedAttributesToRename(syntax);

            var replacement = syntax.Name.HasNamespace ? syntax.Name.LocalName : "";

            var workUnits = new List<IWorkUnit>();
            foreach (var ns in namespaces)
            {
                if (ns != syntax)
                {
                    workUnits.Add(new DeleteXmlSyntaxWorkUnit()
                    {
                        FilePath = document.FilePath,
                        Syntax = ns,
                    });

                    var target = ns.Name.HasNamespace ? ns.Name.LocalName : "";
                    var changes = RenameNamespaceGenerator.RenameNamespace(target, replacement, document, context.XamlSemanticModel, false);

                    if (changes != null && changes.Any())
                    {
                        workUnits.AddRange(changes);
                    }
                }
            }

            return workUnits;
		}
	}
}

