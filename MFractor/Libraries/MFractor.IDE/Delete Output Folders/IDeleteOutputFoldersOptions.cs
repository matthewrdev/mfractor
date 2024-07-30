namespace MFractor.Ide.DeleteOutputFolders
{
    public interface IDeleteOutputFoldersOptions
    {
        bool DeleteBin { get; }

        bool DeleteObj { get; }

        bool DeletePackages { get; }

        bool DeleteVisualStudioWorkingFolder { get; }
    }
}
