using System;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Importing.Generators
{
    public interface IAndroidAdaptiveIconGenerator : ICodeGenerator
    {
        /// <summary>
        /// The output folder where the adaptive icon xml files will be placed.
        /// </summary>
        string AdaptiveIconFolder { get; set; }

        /// <summary>
        /// The file name of the adaptive icon xml file.
        /// </summary>
        string AdaptiveIconRoundName { get; set; }

        /// <summary>
        /// The file name of the adaptive icon xml file.
        /// </summary>
        string AdaptiveIconName { get; set; }

        /// <summary>
        /// The virtual file path, including the Resources folder, where the round adaptive icon xml is place.
        /// </summary>
        string AdaptiveIconRoundPath { get;  }

        /// <summary>
        /// The virtual file path, including the Resources folder, where the adaptive icon xml is place.
        /// </summary>
        string AdaptiveIconPath { get; }

        ICodeSnippet IconTemplate { get; set; }

        ICodeSnippet IconRoundTemplate { get; set; }

        CreateProjectFileWorkUnit GenerateIcon(Project project, string launcherForegroundIconName, string launcherBackgroundColor);

        CreateProjectFileWorkUnit GenerateRoundIcon(Project project, string launcherForegroundIconName, string launcherBackgroundColor);
    }
}
