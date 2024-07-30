using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.WorkUnits;
using MFractor.Commands;
using MFractor.Ide.Commands;
using MFractor.Work;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.Scaffolding
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ScaffolderCommand))]
    class ScaffolderCommand : ICommand
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        public string Name => "Scaffolding Wizard";

        public string Identifier => "com.mfractor.commands.scaffolder";

        [ImportingConstructor]
        public ScaffolderCommand(Lazy<IWorkEngine> workEngine,
                                 Lazy<IProjectService> projectService)
        {
            this.workEngine = workEngine;
            this.projectService = projectService;
        }

        public void Execute(ICommandContext commandContext)
        {
            WorkEngine.ApplyAsync(new ScaffolderWorkUnit()
            {
                Context = CreateScaffoldingContext(commandContext),
                InputValue = GetScaffoldingInput(commandContext)
            });
        }

        string GetScaffoldingInput(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext)
            {
                switch (solutionPadCommandContext.SelectedItem)
                {
                    case IProjectFolder folder:
                        return folder.VirtualPath + Path.DirectorySeparatorChar;
                }
            }
            else if (commandContext is IDocumentCommandContext documentCommandContext)
            {
                var projectFile = ProjectService.GetProjectFileWithFilePath(documentCommandContext.CompilationProject, documentCommandContext.FilePath);

                if (projectFile != null && projectFile.ProjectFolders.Any())
                {
                    return string.Join(Path.DirectorySeparatorChar.ToString(), projectFile.ProjectFolders) + Path.DirectorySeparatorChar.ToString();
                }
            }

            return string.Empty;
        }

        IScaffoldingContext CreateScaffoldingContext(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext)
            {
                switch (solutionPadCommandContext.SelectedItem)
                {
                    case Project project:
                        return new ScaffoldingContext(project);
                    case IProjectFolder folder:
                        return new ScaffoldingContext(folder.Project);
                }
            }
            else if (commandContext is IDocumentCommandContext documentCommandContext)
            {
                return new ScaffoldingContext(documentCommandContext.CompilationProject)
                {

                };
            }

            return default;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var context = CreateScaffoldingContext(commandContext);

            if (context == null)
            {
                return default;
            }

            return new CommandState()
            {
                BlockSubsequentCommands = true,
                Label = "Scaffold",
                Description = "Launch MFractor's scaffolding tool to generate new code files."
            };
        }
    }
}
