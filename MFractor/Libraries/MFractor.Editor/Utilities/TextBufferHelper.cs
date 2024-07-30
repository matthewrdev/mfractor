using System.Linq;
using MFractor.Ide;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;

#if VS_MAC
using MFractor.VS.Mac.Utilities;
using DotNetProject = MonoDevelop.Projects.DotNetProject;
#endif

namespace MFractor.Editor.Utilities
{
    public static class TextBufferHelper
    {
        /// <summary>
        /// Gets the <see cref="Project"/> that is associated with the provided <paramref name="textBuffer"/>.
        /// </summary>
        public static Project GetCompilationProject(ITextBuffer textBuffer)
        {
            if (textBuffer is null)
            {
                return default;
            }

            var documents = textBuffer.GetRelatedDocuments().ToList();

            var bufferProject = documents.FirstOrDefault()?.Project;

            if (bufferProject == null)
            {
                return GetActiveDocumentCompilationProject();
            }

            if (!bufferProject.SupportsCompilation)
            {
                var cleanedName = CleanProjectName(bufferProject.Name);
                foreach (var project in bufferProject.Solution.Projects)
                {
                    // TODO: Match the target with the 
                    if (project.Name == cleanedName)
                    {
                        return project;
                    }
                }

                // TODO: This needs to updated for Windows to support MAUI.
#if VS_MAC
                // No match on cleaned name, try matching based on active exectution target (for MAUI apps).
                var executionTargetPreprocessorFilter = GetActiveExecutionTargetFilter();
                if (!string.IsNullOrEmpty(executionTargetPreprocessorFilter))
                {
                    foreach (var project in bufferProject.Solution.Projects)
                    {
                        if (project.ParseOptions == null)
                        {
                            continue;
                        }

                        if (project.Name.StartsWith(cleanedName)
                            && project.ParseOptions.PreprocessorSymbolNames.Contains(executionTargetPreprocessorFilter))
                        {
                            return project;
                        }
                    }
                }
#endif


                return GetActiveDocumentCompilationProject();
            }
            else if (string.IsNullOrEmpty(bufferProject.FilePath))
            {
                // 

            }

            return bufferProject;
        }

#if VS_MAC
        public static string GetActiveExecutionTargetFilter()
        {
            var executionTarget = MonoDevelop.Ide.IdeApp.Workspace.ActiveExecutionTarget;
            if (executionTarget == null)
            {
                return string.Empty;
            }

            return GetExecutionTargetFilter(executionTarget);
        }

        public static string GetExecutionTargetFilter(MonoDevelop.Core.Execution.ExecutionTarget executionTarget)
        {
            if (executionTarget == null)
            {
                return string.Empty;
            }

            var type = executionTarget.GetType();
            if (executionTarget.Id == "MyAppleMacCatalystID"
                || executionTarget.Name.StartsWith("mac", StringComparison.InvariantCultureIgnoreCase))
            {
                return "MACCATALYST";
            }

            if (executionTarget.Id.StartsWith("android", StringComparison.InvariantCultureIgnoreCase)
                || executionTarget.Name.StartsWith("android", StringComparison.InvariantCultureIgnoreCase))
            {
                return "ANDROID";
            }

            if (executionTarget.Id.StartsWith("ios", StringComparison.InvariantCultureIgnoreCase)
                ||  executionTarget.Name.StartsWith("ios", StringComparison.InvariantCultureIgnoreCase))
            {
                return "IOS";
            }

            if (executionTarget.ParentGroup != null)
            {
                return GetExecutionTargetFilter(executionTarget.ParentGroup);
            }


            return string.Empty;
        }
#endif

        static Project GetActiveDocumentCompilationProject()
        {
#if VS_MAC
            if (MonoDevelop.Ide.IdeApp.Workbench.ActiveDocument?.Owner is DotNetProject dotNetProject)
            {
                return dotNetProject.ToCompilationProject();
            }

            if (MonoDevelop.Ide.IdeApp.Workbench.ActiveDocument?.Owner is MonoDevelop.Projects.SharedAssetsProjects.SharedAssetsProject sharedAssetsProject)
            {
                var executionTarget = MonoDevelop.Ide.IdeApp.Workspace.ActiveExecutionTarget;

                return SolutionHelper.ToCompilationProject(sharedAssetsProject);
            }

            return  (MonoDevelop.Ide.IdeApp.Workbench.ActiveDocument?.Owner as DotNetProject)?.ToCompilationProject();
#else
            // Emergency fallback option.
            var activeDocument = MFractor.IOC.Resolver.Resolve<IActiveDocument>();
            if (activeDocument != null && activeDocument.IsAvailable && activeDocument.CompilationProject != null)
            {
                return activeDocument.CompilationProject;
            }

            return default;
#endif
        }

        static string CleanProjectName(string name)
        {
            if (name.EndsWith("-XamlProject"))
            {
                return name.Substring(0, name.Length - "-XamlProject".Length);
            }

            if (name.EndsWith("-Xaml"))
            {
                return name.Substring(0, name.Length - "-Xaml".Length);
            }

            return name;
        }
    }
}
