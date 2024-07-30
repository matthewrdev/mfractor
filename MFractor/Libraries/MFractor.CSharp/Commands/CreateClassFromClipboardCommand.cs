using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.CSharp.CodeGeneration.ClassFromClipboard;
using MFractor.CSharp.WorkUnits;
using MFractor.Ide.Commands;
using MFractor.Work;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class CreateClassFromClipboardCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IClipboard> clipboard;
        public IClipboard Clipboard => clipboard.Value;

        readonly Lazy<IClassFromStringContentGenerator> classFromStringContentGenerator;
        public IClassFromStringContentGenerator ClassFromStringContentGenerator => classFromStringContentGenerator.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "Create Class From Clipboard";

        [ImportingConstructor]
        public CreateClassFromClipboardCommand(Lazy<IClipboard> clipboard,
                                               Lazy<IClassFromStringContentGenerator> classFromStringContentGenerator,
                                               Lazy<IWorkEngine> workEngine)
        {
            this.clipboard = clipboard;
            this.classFromStringContentGenerator = classFromStringContentGenerator;
            this.workEngine = workEngine;
        }

        public void Execute(ICommandContext commandContext)
        {
            var project = GetProject(commandContext);
            var folderPath = GetFolderPath(commandContext);
            var code = Clipboard.Text;

            ClassFromStringContentGenerator.GetTypeAndNamespaceSyntax(code, out var type, out var @namespace);

            var unit = new CreateClassFromContentWorkUnit(type.Identifier.ValueText,
                                                          project,
                                                          folderPath,
                                                          code);

            WorkEngine.ApplyAsync(unit).ConfigureAwait(false);
        }

        Project GetProject(ICommandContext commandContext)
        {
            var solutionPadContext = commandContext as ISolutionPadCommandContext;

            if (solutionPadContext == null)
            {
                return default;
            }

            if (solutionPadContext.SelectedItem is IProjectFolder projectFolder)
            {
                return projectFolder.Project;
            }

            return solutionPadContext.SelectedItem as Project;
        }

        string GetFolderPath(ICommandContext commandContext)
        {
            var solutionPadContext = commandContext as ISolutionPadCommandContext;

            if (solutionPadContext == null)
            {
                return string.Empty;
            }

            if (solutionPadContext.SelectedItem is IProjectFolder projectFolder)
            {
                return projectFolder.VirtualPath;
            }

            return string.Empty;
        }


        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var solutionPadContext = commandContext as ISolutionPadCommandContext;

            if (solutionPadContext == null)
            {
                return default;
            }

            var isCompatible = solutionPadContext.SelectedItem is IProjectFolder || solutionPadContext.SelectedItem is Project;
            if (!isCompatible)
            {
                return default;
            }

            var clipboardContent = Clipboard.Text;

            if (!ClassFromStringContentGenerator.CanCreateClassFromContent(clipboardContent))
            {
                return default;
            }

            return new CommandState()
            {
                Label = "Add Class Using Clipboard",
                Description = "Create a new class file using the clipboards contents",
            };
        }
    }
}