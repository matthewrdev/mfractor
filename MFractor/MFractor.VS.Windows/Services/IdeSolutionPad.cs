using System;using System.ComponentModel.Composition;using System.Linq;
using EnvDTE;using EnvDTE80;
using MFractor.Ide;
using MFractor.VS.Windows.Utilities;using MFractor.VS.Windows.WorkspaceModel;
using MFractor.Workspace;
using Microsoft.VisualStudio.Shell;

namespace MFractor.VS.Windows.Services{    [PartCreationPolicy(CreationPolicy.Shared)]    [Export(typeof(ISolutionPad))]    class IdeSolutionPad : ISolutionPad    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IWorkspaceShadowModel> workspaceShadowModel;
        IWorkspaceShadowModel WorkspaceShadowModel => workspaceShadowModel.Value;

        [ImportingConstructor]
        public IdeSolutionPad(Lazy<IWorkspaceShadowModel> workspaceShadowModel)
        {
            this.workspaceShadowModel = workspaceShadowModel;
        }

        DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        UIHierarchyItem GetSelectedItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var selectedItems = (object[])DTE.ToolWindows.SolutionExplorer.SelectedItems;
            return selectedItems
                .Cast<UIHierarchyItem>()
                .FirstOrDefault();
        }

        /// <summary>
        /// Retrieves the current selected file in the Solution Explorer.
        /// </summary>
        /// <returns></returns>
        ProjectItem GetSelectedProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var selectedItem = GetSelectedItem();

            return selectedItem != null && selectedItem.Object is ProjectItem item
                ? item
                : null;
        }

        /// <summary>
        /// Retrieves the current selected file in the Solution Explorer.
        /// May be used in Commands to identify if it was invoked from the Solution Explorer.
        /// </summary>
        /// <returns>An object representing the selected project file in the Solution Explorer.</returns>
        IProjectFile GetSelectedProjectFile()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var selectedItem = GetSelectedProjectItem();

            if (selectedItem == null)
            {
                return default;
            }

            var guid = DteProjectHelper.GetProjectGuid(selectedItem.ContainingProject);
            var filePath = DteProjectHelper.GetProjectItemFilePath(selectedItem);

            return WorkspaceShadowModel.GetProjectByGuid(guid)?.GetProjectFile(filePath);
        }        public object SelectedItem        {            get            {
                if (!ThreadHelper.CheckAccess())
                {
                    log?.Warning("Trying to access the solution pad on a non-UI thread.");
                    return default;
                }

                var selectedItem = GetSelectedItem();                if (selectedItem.Object is Solution solution)                {                    return solution.ToCompilationSolution();                }                else if (selectedItem.Object is Project project)                {                    // TODO: Detect shared assets projects.                    return project.ToCompilationProject();                }                else if (selectedItem.Object is ProjectItem projectItem)
                {
                    if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFolder)
                    {
                        return new IdeProjectFolder(projectItem);
                    }

                    return GetSelectedProjectFile();
                }                // else if (selectedItem is ProjectFile projectFile)                // {                //  return new IdeProjectFile(projectFile, projectFile.CompilationProject.ToCompilationProject());                // }                return null;            }        }    }}