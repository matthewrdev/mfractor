using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using MFractor.Code.Documents;
using MFractor.Code.Formatting;
using MFractor.CodeSnippets;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using MFractor.Localisation.CodeGeneration;
using MFractor.Localisation.Utilities;
using MFractor.Localisation.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation.LocaliserRefactorings
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class CSharpLocaliserRefactoring : Configurable, ILocaliserRefactoring
    {
        class CSharpLocalisationValue : ILocalisationValue
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

        readonly Lazy<IProjectService> projectService;
        IProjectService ProjectService => projectService.Value;

        readonly Lazy<IConfigurationEngine> configurationEngine;
        IConfigurationEngine ConfigurationEngine => configurationEngine.Value;

        readonly Lazy<IXmlFormattingPolicyService> formattingPolicyService;
        IXmlFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        [ImportingConstructor]
        public CSharpLocaliserRefactoring(Lazy<IProjectService> projectService,
                                          Lazy<IConfigurationEngine> configurationEngine,
                                          Lazy<IXmlFormattingPolicyService> formattingPolicyService,
                                          Lazy<IXmlSyntaxWriter> xmlSyntaxWriter)
        {
            this.projectService = projectService;
            this.configurationEngine = configurationEngine;
            this.formattingPolicyService = formattingPolicyService;
            this.xmlSyntaxWriter = xmlSyntaxWriter;
        }

        [Import]
        public IResXFileGenerator ResXFileGenerator { get; set; }

        public override string Identifier => "com.mfractor.localisation.csharp.refactor";

        public override string Name => "";

        public override string Documentation => "";

        const string codeSnippetDescription = "The localisation expression that replaces the string literal.";

        [ExportProperty(codeSnippetDescription)]
        [CodeSnippetDefaultValue("$symbol$.$key$", codeSnippetDescription)]
        [CodeSnippetArgument("symbol", "The fully qualified symbol name of the symbol that performs localisation.")]
        [CodeSnippetArgument("key", "The key of the localised string")]
        public ICodeSnippet LocalisationCodeSnippet { get; set; }

        [ExportProperty("When no RESX files exist in the project, the default file to create.")]
        public string DefaultLocalisationFilePath { get; set; } = "Resources/Resources.resx";

        public bool IsAvailable(Project project, string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || project == null)
            {
                return false;
            }

            var extension = Path.GetExtension(filePath);

            if (!extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var projectFile = ProjectService.FindProjectFile(project, path => Path.GetExtension(path) == ".resx");

            return projectFile != null;
        }

        public string CreateLocalisationExpression(ICodeSnippet localisationSnippet, LocalisationOperation operation, Project project, IParsedDocument document)
        {
            var filePath = operation.ShouldCreatedDefaultLocalisationFile ? DefaultLocalisationFilePath : operation.AllResourceFiles.FirstOrDefault();

            var fi = new FileInfo(filePath);
            var projectFile = ProjectService.GetProjectFileWithFilePath(project, filePath);

            if (projectFile == null)
            {
                return null;
            }

            var folderPath = "";

            if (projectFile.ProjectFolders.Any())
            {
                folderPath = string.Join(".", projectFile.ProjectFolders);
            }

            var symbolName = ProjectService.GetDefaultNamespace(project, document.FilePath) + ".";

            if (!string.IsNullOrEmpty(folderPath))
            {
                symbolName += folderPath + ".";
            }

            var name = Path.GetFileNameWithoutExtension(fi.Name);

            var extension = Path.GetExtension(name);
            while (!string.IsNullOrEmpty(extension))
            {
                name = Path.GetFileNameWithoutExtension(name);
                extension = Path.GetExtension(name);
            }

            symbolName += name;

            localisationSnippet.SetArgumentValue("symbol", symbolName)
                               .SetArgumentValue("key", operation.Key);

            return localisationSnippet.ToString();
        }

        public IReadOnlyList<IWorkUnit> CreateLocalisationValues(LocalisationOperation operation, Project project, IParsedDocument document)
        {
            var workUnits = new List<IWorkUnit>();
            if (operation.ShouldCreatedDefaultLocalisationFile)
            {
                var value = new CSharpLocalisationValue()
                {
                    Key = operation.Key,
                    Value = operation.Value,
                    Comment = operation.Value,
                };

                workUnits.AddRange(ResXFileGenerator.GenerateResourceFile(project, DefaultLocalisationFilePath, value.AsList(), true));
            }
            else
            {
                var configId = ConfigurationId.Create(project.GetIdentifier());

                var entryGenerator = ConfigurationEngine.Resolve<IResXEntryGenerator>(configId);

                foreach (var filePath in operation.SelectedResourceFiles)
                {
                    var node = entryGenerator.GenerateSyntax(operation.Key, operation.Value, operation.Comment);

                    var policy = FormattingPolicyService.GetXmlFormattingPolicy();

                    var resxCode = XmlSyntaxWriter.WriteNode(node, policy.ContentIndentString, policy, true, true, true);

                    var offset = ResxFileHelper.GetClosingTagOffset(filePath);

                    workUnits.Add(new InsertTextWorkUnit("\n" + resxCode, offset, filePath));
                }
            }

            return workUnits;
        }

        public IReadOnlyList<IWorkUnit> CreateLocalisationExpressionImportStatement(ICodeSnippet localisationSnippet, LocalisationOperation operation, Project project, IParsedDocument document)
        {
            return Array.Empty<IWorkUnit>();
        }
    }
}