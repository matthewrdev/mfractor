using System;
using System.ComponentModel.Composition;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.Ide.Commands;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Android.Commands
{
    [RequiresLicense]
    [Export]
    class OpenAndroidManifestCommand : ICommand
    {
        readonly Lazy<IAndroidManifestResolver> androidManifestResolver;
        public IAndroidManifestResolver AndroidManifestResolver => androidManifestResolver.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public OpenAndroidManifestCommand(Lazy<IAndroidManifestResolver> androidManifestResolver,
                                          Lazy<IWorkEngine> workEngine)
        {
            this.androidManifestResolver = androidManifestResolver;
            this.workEngine = workEngine;
        }

        Project GetTargetProject(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is Project project
                && project.IsAndroidProject())
            {
                return project;
            }

            return default;
        }

        IProjectFile GetAndroidManifest(ICommandContext commandContext)
        {
            var project = GetTargetProject(commandContext);

            if (project is null)
            {
                return default;
            }

            return AndroidManifestResolver.ResolveAndroidManifest(project);
        }

        public void Execute(ICommandContext commandContext)
        {
            var manifest = GetAndroidManifest(commandContext);

            WorkEngine.ApplyAsync(new OpenFileWorkUnit(manifest.FilePath, manifest.CompilationProject)).ConfigureAwait(false);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var manifest = GetAndroidManifest(commandContext);

            if (manifest is null)
            {
                return default;
            }

            return new CommandState("Open Android Manifest", "Opens the Android Manifest file for this Android project.");
        }
    }
}