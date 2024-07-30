using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Configuration.Attributes;
using MFractor.Maui.CodeGeneration.Views;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor
{
    class ExtractResourceDictionary : RefactorXamlCodeAction
    {
        public const string Tip = "You can use the Extract Resource Dictionary refactoring to move a resource dictionary into its own file.";
        readonly IXmlSyntaxWriter xmlSyntaxWriter;

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.extract_resource_dictionary";

        public override string Name => "Extract Resource Dictionary";

        public override string Documentation => "Extracts the current resource dictionary into it's own XAML file with code behind.";

        [Import]
        public IXamlViewWithCodeBehindGenerator XamlViewGenerator { get; set; }

        [ExportProperty("What is the default name of the XAML namespace for the new control?")]
        public string DefaultXamlNamespace { get; set; } = "resources";

        [ExportProperty("What is the default folder that the new  of the C# namespace for the new resource? If empty, the projects default namespace will be used.")]
        public string DefaultResourcesFolder { get; set; } = "Resources";

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        [ImportingConstructor]
        public ExtractResourceDictionary(IXmlSyntaxWriter xmlSyntaxWriter)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
        }

        public override bool CanExecute(XmlNode syntax,
                                        IParsedXamlDocument document,
                                        IXamlFeatureContext context,
                                        InteractionLocation location)
        {
            var type = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(type,context.Platform.ResourceDictionary.MetaType))
            {
                return false;
            }

            if (!syntax.HasChildren)
            {
                return false;
            }

            var hasInnerValues = syntax.Children.Any(c => !XamlSyntaxHelper.IsPropertySetter(c));

            return hasInnerValues;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax,
                                                                   IParsedXamlDocument document,
                                                                   IXamlFeatureContext context,
                                                                   InteractionLocation location)
        {
            return CreateSuggestion("Extract Resource Dictionary").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax,
                                                       IParsedXamlDocument document,
                                                       IXamlFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            var type = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            IReadOnlyList<IWorkUnit> callback(GenerateCodeFilesResult result)
            {
                var namespaceValue = ProjectService.GetDefaultNamespace(result.SelectedProject);

                if (string.IsNullOrEmpty(result.FolderPath) == false)
                {
                    var folderPath = result.FolderPath.Replace("/", ".").Replace("\\", ".").Replace(" ", "");
                    namespaceValue += "." + folderPath;
                }

                var options = FormattingPolicyService.GetFormattingPolicy(context);

                var xmlPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

                var workUnits = XamlViewGenerator.Generate(result.Name,
                                                           namespaceValue,
                                                           result.SelectedProject,
                                                           result.FolderPath,
                                                           xmlPolicy,
                                                           syntax,
                                                           context.Platform,
                                                           context.Namespaces,
                                                           type).ToList();

                var inner = new XmlNode
                {
                    Name = new XmlName($"{DefaultXamlNamespace}:{result.Name}"),
                    IsSelfClosing = true
                };

                var merged = new XmlNode
                {
                    Name = new XmlName(syntax.Name.FullName + ".MergedDictionaries")
                };
                merged.AddChildNode(inner);
                merged.IsSelfClosing = false;

                var resourceDictionary = new XmlNode
                {
                    Name = new XmlName(syntax.Name.FullName)
                };
                resourceDictionary.AddChildNode(merged);

                var xmlWorkUnit = new ReplaceXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Existing = syntax,
                    New = resourceDictionary,
                    ReplaceChildren = true,
                    GenerateClosingTags = true,
                };

                workUnits.Add(xmlWorkUnit);

                var attr = XamlNamespaceImportGenerator.GenerateXmlnsImportAttibute(DefaultXamlNamespace, namespaceValue, null, false);

                var importStatement = " " + xmlSyntaxWriter.WriteAttribute(attr, xmlPolicy);

                workUnits.Add(new InsertTextWorkUnit(importStatement, document.GetSyntaxTree().Root.OpeningTagSpan.End - 1, document.FilePath));

                return workUnits;
            }


            var workUnit = new GenerateCodeFilesWorkUnit("",
                                                         context.Project,
                                                         new List<Project>() { context.Project },
                                                         DefaultResourcesFolder,
                                                         "Extract Resource Dictionary",
                                                         "Enter the name of the new resource dictionary",
                                                         string.Empty,
                                                         ProjectSelectorMode.Single,
                                                         callback);

            return workUnit.AsList();
        }
    }
}
