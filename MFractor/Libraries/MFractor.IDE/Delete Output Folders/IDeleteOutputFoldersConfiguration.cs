namespace MFractor.Ide.DeleteOutputFolders
{
    public interface IDeleteOutputFoldersConfiguration
    {
        string Name { get; }

        string Identifier { get; }

        IDeleteOutputFoldersOptions Options { get; }
    }
}
