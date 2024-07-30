using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.StaticResources;
using MFractor.Maui.CodeGeneration.ResourceDictionaries;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Fix.StaticResources
{
    class ImportResourceDictionaryFix : FixCodeAction
    {
        public override string Documentation => "WHen ";

        public override Type TargetCodeAnalyser => typeof(UndefinedStaticResourceAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.import_resource_dictionary";

        public override string Name => "Import Resource Dictionary";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public IImportResourceDictionaryGenerator ImportResourceDictionaryGenerator { get; set; }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (!syntax.HasValue)
            {
                return false;
            }

            var bundle = issue.GetAdditionalContent<UndefinedStaticResourceBundle>();

            return bundle != null
                   && bundle.AvailableResourceDictionaries != null
                   && bundle.AvailableResourceDictionaries.Any();
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var dictionaries = issue.GetAdditionalContent<List<INamedTypeSymbol>>();

            var suggestions = new List<ICodeActionSuggestion>();

            foreach (var d in dictionaries)
            {
                suggestions.Add(CreateSuggestion("Import " + d.MetadataName, dictionaries.IndexOf(d)));
            }

            return suggestions;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return null;

            //var type = issue.GetAdditionalContent<INamedTypeSymbol>();


            //SymbolHelper.ExplodeTypeName(type.ToString(), out var @namespace, out var typeName);

            //var namespaceValue = ProjectService.GetDefaultNamespace(result.Project);

            //var inner = new XmlNode
            //{
            //    Name = new XmlName($"resources:{typeName}"),
            //    IsSelfClosing = true
            //};

            //var merged = new XmlNode
            //{
            //    Name = new XmlName(syntax.Name.FullName + ".MergedDictionaries")
            //};
            //merged.AddChildNode(inner);
            //merged.IsSelfClosing = false;

            //var resourceDictionary = new XmlNode
            //{
            //    Name = new XmlName(syntax.Name.FullName)
            //};
            //resourceDictionary.AddChildNode(merged);

            //var xmlWorkUnit = new ReplaceXmlSyntaxWorkUnit(xmlPolicy)
            //{
            //    FilePath = document.FilePath,
            //    TargetProject = context.Project,
            //    Existing = syntax,
            //    New = resourceDictionary,
            //    ReplaceChildren = true,
            //    GenerateClosingTags = true,
            //};

            //workUnits.Add(xmlWorkUnit);

            //var attr = CodeGenerationHelper.GenerateXmlnsImportAttibute("resources", namespaceValue, null, false);

            //var importStatement = " " + XmlSyntaxWriter.WriteAttribute(attr, xmlPolicy);

            //workUnits.Add(new InsertTextWorkUnit(importStatement, document.GetSyntaxTree().Root.OpeningTagSpan.End - 1, document.FilePath));

            //return workUnits;

            //var symbol = issue.GetAdditionalContent<ISymbol>();
            //var newName = GenerateValueName(syntax.Value.Value, symbol);
            //return new ReplaceTextWorkUnit(document.FilePath, newName, syntax.Value.Span);
        }
    }
}

