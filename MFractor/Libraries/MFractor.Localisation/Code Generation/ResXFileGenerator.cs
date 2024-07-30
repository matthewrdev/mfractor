using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IResXFileGenerator))]
    class ResXFileGenerator : CodeGenerator, IResXFileGenerator
    {
        public override string[] Languages { get; } = new string[] { "RESX" };

        public override string Identifier => "com.mfractor.code_gen.resx.resx_file";

        public override string Name => "Generate ResX File";

        public override string Documentation => "Generates a new ResX file";

        [Import]
        public IResXEntryGenerator ResXEntryGenerator { get; set; }

        [CodeSnippetResource("Resources/Snippets/ResxTemplate.txt")]
        [ExportProperty("The default .resx file template.")]
        [CodeSnippetArgument("values", "The values to insert")]
        public ICodeSnippet ResXTemplate { get; set; }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace of the ")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new resources designer class.")]
        [CodeSnippetResource("Resources/Snippets/ResxDesignerTemplate.txt")]
        [ExportProperty("The default designer file template for a .resx resource.")]
        public ICodeSnippet ResXDesignerTemplate { get; set; }

        public IReadOnlyList<IWorkUnit> GenerateResourceFile(Project project,
                                                           string resourceFilePath,
                                                           bool includeDesignerFile)
        {
            return GenerateResourceFile(project, resourceFilePath, Enumerable.Empty<ILocalisationValue>(), includeDesignerFile);

        }

        public IReadOnlyList<IWorkUnit> GenerateResourceFile(Project project,
                                                           string resourceFilePath,
                                                           IEnumerable<ILocalisationValue> values,
                                                           bool includeDesignerFile)
        {
            var fileInfo = new FileInfo(resourceFilePath);
            var fileName = fileInfo.Name;

            var directory = Path.GetDirectoryName(resourceFilePath);

            var resourceNamespace = ProjectService.GetDefaultNamespace(project);

            if (!string.IsNullOrEmpty(directory))
            {
                resourceNamespace += "." + directory.Replace("\\", ".").Replace("/", ".");
            }

            var result = new List<IWorkUnit>();

            var resxFilePath = resourceFilePath;

            if (includeDesignerFile)
            {
                var designerFilePath = Path.GetFileNameWithoutExtension(fileName);
                var designerFileName = designerFilePath;

                if (!string.IsNullOrEmpty(directory))
                {
                    designerFilePath = Path.Combine(directory, designerFilePath);
                }

                ResXDesignerTemplate.SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, resourceNamespace)
                                    .SetArgumentValue(ReservedCodeSnippetArgumentName.Name, designerFileName);

                result.Add(new CreateProjectFileWorkUnit
                {
                    BuildAction = "Compile",
                    FilePath = designerFilePath + ".Designer.cs",
                    DependsUponFile = resxFilePath,
                    FileContent = ResXDesignerTemplate.ToString(),
                    TargetProject = project,
                    AllowPostProcessing = false
                });
            }

            var content = "";
            if (values != null && values.Any())
            {
                content = Environment.NewLine;
                foreach (var v in values)
                {
                    content += ResXEntryGenerator.GenerateCode(v.Key, v.Value.Trim(), v.Comment, "  ");
                    content += Environment.NewLine;
                }
            }

            ResXTemplate.SetArgumentValue("values", content);

            result.Add(new CreateProjectFileWorkUnit()
            {
                BuildAction = "EmbeddedResource",
                ResourceId = resourceNamespace + "." + Path.GetFileNameWithoutExtension(fileName) + ".resources",
                FilePath = resxFilePath,
                FileContent = ResXTemplate.ToString(),
                TargetProject = project,
                Generator = includeDesignerFile ? "ResXFileCodeGenerator" : string.Empty
            });

            return result;
        }
    }
}
