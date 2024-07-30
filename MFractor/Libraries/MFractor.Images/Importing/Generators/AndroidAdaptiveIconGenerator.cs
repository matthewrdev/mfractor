using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Images.Importing.Generators;
using MFractor.Utilities;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MSBuildProjectName.Replace_.Importing.Generators
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAndroidAdaptiveIconGenerator))]
    class AndroidAdaptiveIconGenerator : CodeGenerator, IAndroidAdaptiveIconGenerator
    {
        [ExportProperty("The folder name within Resources where the adaptive icons should be added.")]
        public string AdaptiveIconFolder { get; set; } = "mipmap-anydpi-v26";

        [ExportProperty("The filename of adaptive icon.")]
        public string AdaptiveIconName { get; set; } = "icon.xml";

        [ExportProperty("The filename of the round adaptive icon.")]
        public string AdaptiveIconRoundName { get; set; } = "icon_round.xml";

        [ExportProperty("The file contents of the adaptive icon.")]
        [CodeSnippetResource("Resources/Android/icon.txt")]
        [CodeSnippetArgument("launcher_foreground", "The name of the launcher foreground mipmap image.")]
        [CodeSnippetArgument("launcher_background", "The color value or lookup to use for the launcher background.")]
        public ICodeSnippet IconTemplate { get; set; }

        [ExportProperty("The file contents of the round adaptive icon.")]
        [CodeSnippetResource("Resources/Android/icon_round.txt")]
        [CodeSnippetArgument("launcher_foreground", "The name of the launcher foreground mipmap image.")]
        [CodeSnippetArgument("launcher_background$", "The color value or lookup to use for the launcher background.")]
        public ICodeSnippet IconRoundTemplate { get; set; }

        public override string[] Languages { get; } = new string[] { "xml" };

        public override string Identifier => "com.mfractor.code_gen.images.android.adaptive_icon_generator";

        public override string Name => "Android Adaptive Icon Generator";

        public override string Documentation => "Generates the XML files for adaptive launcher icons on Android";

        public string AdaptiveIconRoundPath => Path.Combine("Resources", AdaptiveIconFolder, AdaptiveIconRoundName);

        public string AdaptiveIconPath => Path.Combine("Resources", AdaptiveIconFolder, AdaptiveIconName);

        public CreateProjectFileWorkUnit GenerateIcon(Project project, string launcherForegroundIconName, string launcherBackgroundColor)
        {
            if (project is null || !project.IsAndroidProject())
            {
                throw new ArgumentException(nameof(project) + " is null or is not an Android project");
            }

            return Generate(project, launcherForegroundIconName, launcherBackgroundColor, IconTemplate, AdaptiveIconName);
        }

        public CreateProjectFileWorkUnit GenerateRoundIcon(Project project, string launcherForegroundIconName, string launcherBackgroundColor)
        {
            if (project is null || !project.IsAndroidProject())
            {
                throw new ArgumentException(nameof(project) + " is null or is not an Android project");
            }

            return Generate(project, launcherForegroundIconName, launcherBackgroundColor, IconRoundTemplate, AdaptiveIconRoundName);
        }

        CreateProjectFileWorkUnit Generate(Project project,
                                           string launcherForegroundIconName,
                                           string launcherBackgroundColor,
                                           ICodeSnippet codeSnippet,
                                           string fileName)
        {
            if (string.IsNullOrEmpty(launcherForegroundIconName))
            {
                throw new ArgumentException($"'{nameof(launcherForegroundIconName)}' cannot be null or empty", nameof(launcherForegroundIconName));
            }

            if (string.IsNullOrEmpty(launcherBackgroundColor))
            {
                throw new ArgumentException($"'{nameof(launcherBackgroundColor)}' cannot be null or empty", nameof(launcherBackgroundColor));
            }

            codeSnippet.SetArgumentValue("launcher_foreground", launcherForegroundIconName)
                       .SetArgumentValue("launcher_background", launcherBackgroundColor);

            var virtualPath = Path.Combine("Resources", AdaptiveIconFolder, fileName);
            var filePath = VirtualFilePathHelper.VirtualProjectPathToDiskPath(project, virtualPath);

            return new CreateProjectFileWorkUnit()
            {
                TargetProject = project,
                FileContent = codeSnippet.ToString(),
                FilePath = filePath
            };
        }
    }
}