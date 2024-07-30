using System;
using System.Collections.Generic;
using MFractor.Workspace;

namespace MFractor.Views.ImageDeletionTool
{
    public class ImagesDeletedEventArgs : EventArgs
    {
        public IReadOnlyList<IProjectFile> DeletedFiles { get; }

        public ImagesDeletedEventArgs(IReadOnlyList<IProjectFile> deletedFiles)
        {
            DeletedFiles = deletedFiles;
        }
    }
}
