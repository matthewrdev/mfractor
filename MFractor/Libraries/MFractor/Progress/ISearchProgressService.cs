using System;

namespace MFractor.Progress
{
    public interface ISearchProgressService
    {
        IProgressMonitor StartSearch(string description);
    }
}