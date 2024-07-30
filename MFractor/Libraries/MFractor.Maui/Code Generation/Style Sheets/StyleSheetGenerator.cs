using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.StyleSheets
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IStyleSheetGenerator))]
    class StyleSheetGenerator : CodeGenerator, IStyleSheetGenerator
    {
        [ExportProperty("The default cascading style sheet to use.")]
        [CodeSnippetResource("Resources/Snippets/StyleSheet.txt")]
        [CodeSnippetArgument("control", "The name of the control that this style sheet is for. EG: `stackLayout`, `entry`, `view` etc")]
        public ICodeSnippet Snippet
        {
            get;
            set;
        }

        [ExportProperty("What is the folder to place new style sheets into?")]
        public string StyleFolder
        {
            get;
            set;
        } = "Styles";


        [ExportProperty("What is the default control to use when generating new style sheets?")]
        public string DefaultControl
        {
            get;
            set;
        } = "view";

        public override string Identifier => "com.mfractor.code_gen.xaml.generate_style_sheet";

        public override string Name => "Generates Cascading Style Sheet";

        public override string Documentation => "Generates a new cascading style sheet that can be used to style XAML";

        public override string[] Languages { get; } = new string[] { "css" };

        public string Generate(string control)
        {
            Snippet.SetArgumentValue("control", control);

            return Snippet.ToString();
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project)
        {
            IReadOnlyList<IWorkUnit> generateCss(GenerateCodeFilesResult result)
            {
                var name = result.Name;
                var filePath = result.Name;
                if (!(name.Contains("/") || name.Contains("\\")))
                {
                    filePath = Path.Combine(result.FolderPath, name + ".css");
                }

                return Generate(DefaultControl, filePath, project).ToList();
            }

            var projects = new List<Project>()
            {
                project,
            };

            return new GenerateCodeFilesWorkUnit("style.css",
                                                       project,
                                                       projects,
                                                       StyleFolder,
                                                       "Create Cascading Style Sheet",
                                                       "Enter a name for the new cascading style sheet",
                                                       string.Empty,
                                                       ProjectSelectorMode.Single,
                                                       generateCss).AsList();
        }

        public IReadOnlyList<IWorkUnit> Generate(string control, string filePath, Project project)
        {
            if (!filePath.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".css";
            }

            var code = Generate(control);

            return new CreateProjectFileWorkUnit()
            {
                FileContent = code,
                BuildAction = "EmbeddedResource",
                FilePath = filePath,
                TargetProject = project,
            }.AsList();
        }

        public string GetFilePathForStyleSheetName(string styleSheetName)
        {
            var filePath = styleSheetName;
            if (!(styleSheetName.Contains("/") || styleSheetName.Contains("\\")))
            {
                filePath = Path.Combine(StyleFolder, styleSheetName);
            }

            if (!filePath.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".css";
            }

            return filePath;
        }
    }
}