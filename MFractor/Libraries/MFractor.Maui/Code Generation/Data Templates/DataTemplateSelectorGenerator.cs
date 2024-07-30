using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.DataTemplates
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDataTemplateSelectorGenerator))]
    class DataTemplateSelectorGenerator : CodeGenerator, IDataTemplateSelectorGenerator
    {
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the new DataTemplateSelector should be placed within.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new DataTemplateSelector")]
        [CodeSnippetResource("Resources/Snippets/DataTemplateSelectorTemplate.txt")]
        [ExportProperty("The code snippet for the DataTemplateSelector")]
        public ICodeSnippet Template { get; set; }

        public override string[] Languages { get; } = { "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.data_template_selector";

        public override string Name => "Data Template Selector Code Generator";

        public override string Documentation => $"The {Name} creates a new DataTemplateSelector that you can fill in.";

        [ExportProperty("The namespace that the data template selector was generated into.")]
        public string Namespace { get; set; } = ".DataTemplateSelectors";

        public string Folder { get; set; } = "Data Template Selectors";

        public IReadOnlyList<IWorkUnit> Generate(Project project, string dataTemplateSelectorName)
        {
            var @namespace = GetNamespaceFor(project, Folder);

            var path = Path.Combine(Folder, dataTemplateSelectorName + ".cs");

            return Generate(project, @namespace, path, Name);
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project, string namespaceName, string filePath, string dataTemplateSelectorName)
        {
            var code = GenerateCode(namespaceName, dataTemplateSelectorName);

            if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".cs";
            }

            return new CreateProjectFileWorkUnit()
            {
                TargetProject = project,
                FileContent = code,
                FilePath = filePath
            }.AsList();
        }

        public string GenerateCode(Project project, string dataTemplateSelectorName)
        {
            var @namespace = GetNamespaceFor(project, Folder);

            return GenerateCode(@namespace, dataTemplateSelectorName);
        }

        public string GenerateCode(string namespaceName, string dataTemplateSelectorName)
        {
            return GenerateSyntax(namespaceName, dataTemplateSelectorName).ToString();
        }

        public CompilationUnitSyntax GenerateSyntax(Project project, string dataTemplateSelectorName)
        {
            var @namespace = GetNamespaceFor(project, Folder);

            return GenerateSyntax(@namespace, dataTemplateSelectorName);
        }

        public CompilationUnitSyntax GenerateSyntax(string namespaceName, string dataTemplateSelectorName)
        {
            Template.SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, namespaceName)
                    .SetArgumentValue(ReservedCodeSnippetArgumentName.Name, dataTemplateSelectorName);

            return Template.AsCompilationUnit();
        }

        public string GetNamespaceFor(Project project, string namespacePath)
        {
            var defaultNamespace = ProjectService.GetDefaultNamespace(project);

            if (string.IsNullOrEmpty(namespacePath))
            {
                return defaultNamespace;
            }

            if (!namespacePath.StartsWith(".", StringComparison.Ordinal))
            {
                namespacePath = "." + namespacePath;
            }

            return defaultNamespace + namespacePath;
        }
    }
}