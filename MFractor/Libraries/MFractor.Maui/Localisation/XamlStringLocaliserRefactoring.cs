using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using MFractor.CodeSnippets;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using MFractor.Code.Documents;
using MFractor.Code;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Localisation;
using MFractor.Localisation.CodeGeneration;
using MFractor.Localisation.LocaliserRefactorings;
using MFractor.Localisation.Utilities;
using MFractor.Localisation.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MFractor.Code.Formatting;
using MFractor.Workspace;

namespace MFractor.Maui.Localisation
{
    class XamlStringLocaliserRefactoring : Configurable, ILocaliserRefactoring
    {
        class XamlLocalisationValue : ILocalisationValue
        {
            public string Key { get; set; }

            public TextSpan? KeySpan => null;

            public string Value { get; set; }

            public TextSpan? ValueSpan => null;

            public CultureInfo Culture => null;

            public bool HasCulture => false;

            public string Comment { get; set; }

            public TextSpan? CommentSpan => null;
        }

        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        public ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService;
        public IXmlFormattingPolicyService XmlFormattingPolicyService => xmlFormattingPolicyService.Value;

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        public override string Identifier => "com.mfractor.localisation.xaml_localisation_refactoring";

        public override string Name => "XAML Localisation Refactoring";

        public override string Documentation => "Converts a string literal into a RESX entry and replaces the original string with a localisation expression.";

        [Import]
        public IResXEntryGenerator ResXEntryGenerator { get; set; }

        [Import]
        public IResXFileGenerator ResXFileGenerator { get; set; }

        const string codeSnippetDescription = "The localisation expression that replaces the string literal.";

        [ExportProperty(codeSnippetDescription)]
        [CodeSnippetDefaultValue("{x:Static $namespace$:$name$.$member$}", codeSnippetDescription)]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The xmlns that contains the localisation class")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the localisation class")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Member, "The resx key")]
        public ICodeSnippet LocalisationCodeSnippet { get; set; }

        [ExportProperty("When no RESX files exist in the project, the default file to create.")]
        public string DefaultLocalisationFilePath { get; set; } = "Resources/Resources.resx";

        [ImportingConstructor]
        public XamlStringLocaliserRefactoring(Lazy<ICodeFormattingPolicyService> formattingPolicyService,
                                              Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                              Lazy<IProjectService> projectService,
                                              Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService)
        {
            this.formattingPolicyService = formattingPolicyService;
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.projectService = projectService;
            this.xmlFormattingPolicyService = xmlFormattingPolicyService;
        }

        public IReadOnlyList<IWorkUnit> CreateLocalisationValues(LocalisationOperation operation, Project project, IParsedDocument document)
        {
            var workUnits = new List<IWorkUnit>();
            if (operation.ShouldCreatedDefaultLocalisationFile)
            {
                var value = new XamlLocalisationValue()
                {
                    Key = operation.Key,
                    Value = operation.Value,
                    Comment = operation.Value,
                };

                workUnits.AddRange(ResXFileGenerator.GenerateResourceFile(project, DefaultLocalisationFilePath, value.AsList(), true));
            }
            else
            {
                foreach (var filePath in operation.SelectedResourceFiles)
                {
                    var node = ResXEntryGenerator.GenerateSyntax(operation.Key, operation.Value, operation.Comment);

                    var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

                    var resxCode = XmlSyntaxWriter.WriteNode(node, policy.ContentIndentString, policy, true, true, true);

                    var offset = ResxFileHelper.GetClosingTagOffset(filePath);

                    workUnits.Add(new InsertTextWorkUnit("\n" + resxCode, offset, filePath));
                }
            }

            return workUnits;
        }

        public bool IsAvailable(Project project, string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || project == null)
            {
                return false;
            }

            var extension = Path.GetExtension(filePath);

            if (!extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var projectFile = ProjectService.GetProjectFilesWithExtension(project, ".resx");

            return projectFile != null;
        }

        public string CreateLocalisationExpression(ICodeSnippet localisationSnippet,
                                                   LocalisationOperation operation,
                                                   Project project,
                                                   IParsedDocument document)
        {
            var chosenFile = operation.SelectedResourceFiles.FirstOrDefault();

            var name = operation.ShouldCreatedDefaultLocalisationFile || string.IsNullOrEmpty(chosenFile) ? DefaultLocalisationFilePath : chosenFile;

            name = Path.GetFileNameWithoutExtension(name).Split('.').First();
            name = CSharpNameHelper.ConvertToValidCSharpName(name);

            return localisationSnippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Member, operation.Key)
                                      .SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, "resources")
                                      .SetArgumentValue(ReservedCodeSnippetArgumentName.Name, name).ToString();
        }

        public IReadOnlyList<IWorkUnit> CreateLocalisationExpressionImportStatement(ICodeSnippet localisationSnippet, LocalisationOperation operation, Project project, IParsedDocument document)
        {
            var xamlDocument = document as IParsedXamlDocument;
            if (xamlDocument is null)
            {
                return Array.Empty<IWorkUnit>();
            }

            var resourcesXmlns = xamlDocument.Namespaces.FirstOrDefault(ns => ns.FullName == "resources");

            if (resourcesXmlns != null)
            {
                return Array.Empty<IWorkUnit>();
            }

            return Array.Empty<IWorkUnit>();
        }
    }
}
