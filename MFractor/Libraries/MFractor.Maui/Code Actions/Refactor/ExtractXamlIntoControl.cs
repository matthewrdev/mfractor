using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Configuration.Attributes;
using MFractor.Maui.CodeGeneration.Views;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Maui.Configuration;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Refactor
{
    class ExtractXamlIntoControl : RefactorXamlCodeAction
    {
        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.extract_xaml_into_control";

        public override string Name => "Extract XAML Into Control";

        public override string Documentation => "Extracts a XAML layout container that derives from `.Layout` into a new XAML control.";

        [Import]
        public IXamlViewWithCodeBehindGenerator XamlViewGenerator { get; set; }

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        [ExportProperty("What is the default name of the XAML namespace for the new control?")]
        public string DefaultXamlNamespace { get; set; } = "controls";

        [Import]
        public ICustomControlsConfiguration CustomControlsConfiguration { get; set; }

        [ImportingConstructor]
        public ExtractXamlIntoControl(Lazy<IXmlSyntaxWriter> xmlSyntaxWriter)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
        }

        public override bool CanExecute(XmlNode syntax,
                                        IParsedXamlDocument document,
                                        IXamlFeatureContext context,
                                        InteractionLocation location)
        {
            if (XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                return false;
            }

            if (syntax.IsRoot)
            {
                return false;
            }

            var type = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (type == null)
            {
                return false;
            }

            return SymbolHelper.DerivesFrom(type, context.Platform.Layout.MetaType);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax,
                                                                  IParsedXamlDocument document,
                                                                  IXamlFeatureContext context,
                                                                  InteractionLocation location)
        {
            return CreateSuggestion("Extract into new XAML control", 0).AsList();
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
                                                           XmlFormattingPolicyService.GetXmlFormattingPolicy(),
                                                           syntax,
                                                           context.Platform,
                                                           context.Namespaces,
                                                           type).ToList();

                var attr = XamlNamespaceImportGenerator.GenerateXmlnsImportAttibute(DefaultXamlNamespace, namespaceValue, null, false);

                var importStatement = " " + XmlSyntaxWriter.WriteAttribute(attr, xmlPolicy);

                workUnits.Add(new InsertTextWorkUnit(importStatement, document.GetSyntaxTree().Root.OpeningTagSpan.End - 1, document.FilePath));

                workUnits.Add(new ReplaceTextWorkUnit()
                {
                    FilePath = document.FilePath,
                    Text = $"<{DefaultXamlNamespace}:{result.Name}/>",
                    Span = syntax.Span,
                });

                return workUnits;
            }

            var workUnit = new GenerateCodeFilesWorkUnit("",
                                                         context.Project,
                                                         context.Project.AsList(),
                                                         CustomControlsConfiguration.ControlsFolder,
                                                         "Extract XAML Control",
                                                         "Enter the name of the new control",
                                                         "http://docs.mfractor.com/xamarin-forms/custom-controls/extracting-custom-controls/",
                                                         ProjectSelectorMode.Single,
                                                         callback);

            return workUnit.AsList();
        }
    }
}