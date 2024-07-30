using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Commands;
using MFractor.Fonts.Utilities;
using MFractor.Maui.CodeGeneration.Fonts;
using MFractor.Ide.Commands;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Commands.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class AddExportFontDeclarationCommand : ICommand
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        readonly Lazy<IExportFontDeclarationGenerator> exportFontDeclarationGenerator;
        public IExportFontDeclarationGenerator ExportFontDeclarationGenerator => exportFontDeclarationGenerator.Value;

        [ImportingConstructor]
        public AddExportFontDeclarationCommand(Lazy<IWorkEngine> workEngine,
                                               Lazy<IExportFontDeclarationGenerator> exportFontDeclarationGenerator,
                                               Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.workEngine = workEngine;
            this.exportFontDeclarationGenerator = exportFontDeclarationGenerator;
            this.xamlPlatforms = xamlPlatforms;
        }

        IProjectFile GetProjectFile(ICommandContext commandContext)
        {
            var solutionPadContext = commandContext as ISolutionPadCommandContext;

            if (solutionPadContext is null)
            {
                return default;
            }

            return solutionPadContext.SelectedItem as IProjectFile;
        }

        public void Execute(ICommandContext commandContext)
        {
            var projectFile = GetProjectFile(commandContext);

            var workUnits = ExportFontDeclarationGenerator.Generate(projectFile.CompilationProject, projectFile.Name).ToList();

            workUnits.Add(new StatusBarMessageWorkUnit()
            {
                Message = "Added an export font declaration for " + projectFile.Name
            });

            WorkEngine.ApplyAsync(workUnits);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var projectFile = GetProjectFile(commandContext);

            if (projectFile is null)
            {
                return default;
            }

            if (!FontAssetHelper.IsFontAsset(projectFile))
            {
                return default;
            }

            if (!projectFile.CompilationProject.TryGetCompilation(out var compilation))
            {
                return default;
            }

            var platform = XamlPlatforms.ResolvePlatform(projectFile.CompilationProject, compilation);
            if (platform is null || !platform.SupportsExportFontAttribute)
            {
                return default;
            }

            var exportFont = compilation.GetTypeByMetadataName(platform.ExportFontAttribute.MetaType);
            if (exportFont is null)
            {
                return default;
            }

            return new CommandState()
            {
                Label = "Add Export Font Declaration",
                Description = $"Adds an {platform.ExportFontAttribute.Name} attribute to this assembly to expose this font to the runtime."
            };
        }
    }
}