using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.CodeGeneration.Fody
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFodyWeaversGenerator))]
    class FodyWeaversGenerator : CodeGenerator, IFodyWeaversGenerator
    {
        public override string[] Languages { get; } = new string[] { "xml" };

        public override string Identifier => "com.mfractor.code_gen.xaml.fody_weavers";

        public override string Name => "FodyWeavers.xml File Generator";

        public override string Documentation => "Generates a FodyWeavers.xml file";

        [ExportProperty("The code snippet to use when creating the fody weavers file")]
        [CodeSnippetResource("Resources/Snippets/FodyWeavers.txt")]
        public ICodeSnippet Snippet
        {
            get;
            set;
        }

        [ExportProperty("The default file path ")]
        public string FodyWeaversFilePath
        {
            get;
            set;
        } = "FodyWeavers.xml";

        public IReadOnlyList<IWorkUnit> Generate(Project project)
        {
            return Generate(project, FodyWeaversFilePath);
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project, string filePath)
        {
            return new CreateProjectFileWorkUnit()
            {
                FileContent = GenerateCode(),
                FilePath = filePath,
                TargetProject = project,
                BuildAction = "None",
            }.AsList();
        }

        public string GenerateCode()
        {
            return Snippet.ToString();
        }

        public XmlSyntaxTree GenerateSyntax()
        {
            return Snippet.AsXmlSyntax();
        }
    }
}