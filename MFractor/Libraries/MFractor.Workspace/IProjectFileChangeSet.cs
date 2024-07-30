using System.Collections.Generic;

namespace MFractor.Workspace
{
    public interface IProjectFileChangeSet<TChange> where TChange : class
    {
        IReadOnlyList<string> ProjectGuids { get; }

        bool ContainsProject(string projectGuid);

        IReadOnlyList<TChange> GetChanges(string projectGuid);
    }
}