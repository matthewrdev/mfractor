using MFractor.Ide.DeleteOutputFolders;

namespace MFractor.Views.DeleteOutputFolders
{
    class MutableDeleteOutputFoldersOptions : IDeleteOutputFoldersOptions
    {
        public bool DeleteBin { get; set; }

        public bool DeleteObj { get; set; }

        public bool DeletePackages { get; set; }

        public bool DeleteVisualStudioWorkingFolder { get; set; }
    }
}
