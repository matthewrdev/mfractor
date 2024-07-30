using System;
using EnvDTE;
using MFractor.Ide;
using MFractor.VS.Windows.Utilities;
using MFractor.Workspace;
using Microsoft.VisualStudio.Shell;

namespace MFractor.VS.Windows.Services
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
        }
                if (!ThreadHelper.CheckAccess())
                {
                    log?.Warning("Trying to access the solution pad on a non-UI thread.");
                    return default;
                }

                var selectedItem = GetSelectedItem();
                {
                    if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFolder)
                    {
                        return new IdeProjectFolder(projectItem);
                    }

                    return GetSelectedProjectFile();
                }