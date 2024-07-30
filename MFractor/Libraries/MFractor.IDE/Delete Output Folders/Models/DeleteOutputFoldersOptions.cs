using System;

namespace MFractor.Ide.DeleteOutputFolders.Models
{
    class DeleteOutputFoldersOptions : IDeleteOutputFoldersOptions
    {
        public DeleteOutputFoldersOptions()
        {
        }

        public DeleteOutputFoldersOptions(IDeleteOutputFoldersOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            DeleteBin = options.DeleteBin;
            DeleteObj = options.DeleteObj;
            DeletePackages = options.DeletePackages;
            DeleteVisualStudioWorkingFolder = options.DeleteVisualStudioWorkingFolder;
        }

        public bool DeleteBin
        {
            get;
            set;
        } = true;

        public bool DeleteObj
        {
            get;
            set;
        } = true;

        public bool DeletePackages
        {
            get;
            set;
        } = false;

        public bool DeleteVisualStudioWorkingFolder
        {
            get;
            set;
        } = false;
    }
}