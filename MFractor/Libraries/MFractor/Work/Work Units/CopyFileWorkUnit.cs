namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// A work unit that triggers a file copy operation from <see 
    /// </summary>
    public class CopyFileWorkUnit : WorkUnit
    {
        public string SourceFilePath { get; set; }

        public string DestinationFilePath { get; set; }

        public bool DeleteSourceFile { get; set; } = false;
    }
}
