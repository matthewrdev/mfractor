using System;

namespace MFractor.Workspace
{
    public interface ISharedAssetsProject
    {
        string FilePath { get; }

        string ProjectItemsFilePath { get; }

        string Guid { get; }

        string Name { get; }
    }
}
