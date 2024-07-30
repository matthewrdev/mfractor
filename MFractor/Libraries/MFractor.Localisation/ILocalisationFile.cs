namespace MFractor.Localisation
{
    public interface ILocalisationFile
    {
        string DisplayPath { get; }
        string FullPath { get; }
        string Extension { get; }
    }
}