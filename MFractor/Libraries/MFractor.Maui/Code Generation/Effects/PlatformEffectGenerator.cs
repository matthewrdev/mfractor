using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Code.Documents;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;

namespace MFractor.Maui.CodeGeneration.CSharp
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IPlatformEffectGenerator))]
    class PlatformEffectGenerator : CodeGenerator, IPlatformEffectGenerator
    {
        [ExportProperty("What is the folder that iOS platform-specific effects should be placed into?")]
        public string IOSEffectsFolder
        {
            get; set;
        } = "Effects";

        [ExportProperty("What is the folder that Android platform-specific effects should be placed into?")]
        public string AndroidEffectsFolder
        {
            get; set;
        } = "Effects";

        [ExportProperty("What is the folder that the effects declaration be placed into?")]
        public string EffectsFolder
        {
            get; set;
        } = "Effects";

        [ExportProperty("What is the name of the resolution group?")]
        public string ResolutionGroupName
        {
            get; set;
        } = "Effects";

        const string effectNameArgument = "name";
        const string effectGroupNameArgument = "group_name";
        const string effectNamespaceArgument = "namespace";
        const string effectPlatformArgument = "platform";

        [CodeSnippetArgument(effectNameArgument, "The name of the new effect.")]
        [CodeSnippetArgument(effectGroupNameArgument, "The resolution group name of the new effect.")]
        [CodeSnippetArgument(effectNamespaceArgument, "The iOS projects default namespace.")]
        [CodeSnippetArgument(effectPlatformArgument, "If you wish to shared the platform-specific effect template, this is the name of the effects platform.")]
        [CodeSnippetResource("Resources/Snippets/PlatformEffectTemplate.txt")]
        [ExportProperty("What is the code snippet for the iOS platform-specific effect?")]
        public ICodeSnippet IOSEffectSnippet
        {
            get; set;
        }

        [CodeSnippetArgument(effectNameArgument, "The name of the new effect.")]
        [CodeSnippetArgument(effectGroupNameArgument, "The resolution group name of the new effect.")]
        [CodeSnippetArgument(effectNamespaceArgument, "The Android projects default namespace.")]
        [CodeSnippetArgument(effectPlatformArgument, "If you wish to shared the platform-specific effect template, this is the name of the effects platform.")]
        [CodeSnippetResource("Resources/Snippets/PlatformEffectTemplate.txt")]
        [ExportProperty("What is the code snippet for the Android platform-specific effect?")]
        public ICodeSnippet AndroidEffectSnippet
        {
            get; set;
        }

        [CodeSnippetArgument(effectNameArgument, "The name of the new effect.")]
        [CodeSnippetArgument(effectGroupNameArgument, "The resolution group name of the new effect.")]
        [CodeSnippetArgument(effectNamespaceArgument, "The common projects default namespace.")]
        [CodeSnippetResource("Resources/Snippets/RoutingEffectTemplate.txt")]
        [ExportProperty("What is the code snippet for the effect?")]
        public ICodeSnippet EffectSnippet
        {
            get; set;
        }

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator
        {
            get;
            set;
        }

        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.csharp.platform_effect";

        public override string Name => "Generate Platform Specific Effect";

        public override string Documentation => "Generates a new effect, creating the effect inside the common project as well as linked Android and iOS projects.";

        CompilationUnitSyntax GenerateSyntax(ICodeSnippet snippet,
                                             string effectName,
                                             string effectNamespace,
                                             string platform)
        {
            snippet.SetArgumentValue(effectNameArgument, effectName);
            snippet.SetArgumentValue(effectNamespaceArgument, effectNamespace);
            snippet.SetArgumentValue(effectGroupNameArgument, ResolutionGroupName);
            snippet.SetArgumentValue(effectPlatformArgument, platform);

            return snippet.AsCompilationUnit();
        }

        public CompilationUnitSyntax GenerateAndroidEffectSyntax(string name,
                                                                 string effectNamespace)
        {
            return GenerateSyntax(AndroidEffectSnippet, name, effectNamespace, "Android");
        }

        public CompilationUnitSyntax GenerateiOSEffectSyntax(string name, string effectNamespace)
        {
            return GenerateSyntax(IOSEffectSnippet, name, effectNamespace, "iOS");
        }

        public CompilationUnitSyntax GenerateEffectSyntax(string name,
                                                          string effectNamespace)
        {
            return GenerateSyntax(EffectSnippet, name, effectNamespace, "");
        }

        public IReadOnlyList<IWorkUnit> Generate(string effectName,
                                               string effectXamlNamespace,
                                               IParsedXmlDocument document,
                                               Project project,
                                               CompilationWorkspace workspace,
                                               OptionSet formattingOptions,
                                               string projectNamespace,
                                               bool shouldCreateCommonEffect,
                                               ReplaceTextWorkUnit insertEffectNameWorkUnit = null)
        {
            var callback = new Func<IReadOnlyList<Project>, List<IWorkUnit>>((projects) =>
            {
                var workUnits = new List<IWorkUnit>();

                var importCode = " " + XamlNamespaceImportGenerator.GenerateXmlnsImportStatement(effectXamlNamespace, projectNamespace, project.AssemblyName, true);

                var import = new ReplaceTextWorkUnit()
                {
                    FilePath = document.FilePath,
                    Text = importCode,
                    Span = new TextSpan(document.GetSyntaxTree().Root.OpeningTagSpan.End - 1, 0),
                };

                workUnits.Add(import);

                if (insertEffectNameWorkUnit != null)
                {
                    workUnits.Add(insertEffectNameWorkUnit);
                }

                foreach (var p in projects)
                {
                    if (p.IsAppleUnifiedProject())
                    {
                        var unit = GenerateiOSEffectSyntax(effectName, p.Name);

                        var filePath = string.IsNullOrEmpty(IOSEffectsFolder) ?
                                           effectName + ".cs"
                                             : Path.Combine(IOSEffectsFolder, effectName + ".cs");

                        workUnits.Add(new CreateProjectFileWorkUnit(unit.ToString(), filePath, p.GetIdentifier()));
                    }
                    else if (p.IsAndroidProject())
                    {
                        var unit = GenerateAndroidEffectSyntax(effectName, p.Name);

                        var filePath = string.IsNullOrEmpty(AndroidEffectsFolder) ?
                                            effectName + ".cs"
                                             : Path.Combine(AndroidEffectsFolder, effectName + ".cs");

                        workUnits.Add(new CreateProjectFileWorkUnit(unit.ToString(), filePath, p.GetIdentifier()));
                    }
                }

                if (shouldCreateCommonEffect)
                {
                    var unit = GenerateEffectSyntax(effectName, projectNamespace);

                    var filePath = string.IsNullOrEmpty(EffectsFolder) ?
                                         effectName + ".cs"
                                         : Path.Combine(EffectsFolder, effectName + ".cs");

                    workUnits.Add(new CreateProjectFileWorkUnit(unit.ToString(), filePath, project.GetIdentifier()));
                }

                return workUnits;
            });

            var solution = project.Solution;

            var iosProjects = solution.Projects.Where(p => p.IsAppleUnifiedProject());
            var androidProjects = solution.Projects.Where(p => p.IsAndroidProject());

            var choices = new List<Project>();
            choices.AddRange(iosProjects);
            choices.AddRange(androidProjects);

            return new ProjectSelectorWorkUnit(choices, "Choose projects to generate the platform-specific effect into.", "Generate Effect", callback).AsList();
        }
    }
}
