using System;

namespace MFractor.Workspace
{
    interface IMutableWorkspaceService : IWorkspaceService
    {
        void NotifyFileChanged(string guid, string filePath);
    }
}