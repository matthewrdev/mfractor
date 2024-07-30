using EnvDTE;

namespace MFractor.VS.Windows.WorkspaceModel
{
    /// <summary>
    /// An extension to the the <see cref="IWorkspaceShadowModel"/> that allows mutation of solutions, projects and project files.
    /// </summary>
    interface IMutableWorkspaceShadowModel : IWorkspaceShadowModel
    {
        void AddSolution(Solution solution);

        void RemoveSolution(string solutionName);

        void AddProjectToSolution(string solutionName, Project project);

        void RemoveProjectFromSolution(string solutionName, string projectGuid);

        void RenameProject(string solutionName, string projectGuid, Project project);

        void AddFileToProject(ProjectItem projectItem, string projectGuid);

        void RemoveFileFromProject(string filePath, string projectGuid);

        void RenameFile(string oldFilePath, string newFilePath, string projectGuid);

        void RenameSolution(Solution solution, string oldName);
    }
}
