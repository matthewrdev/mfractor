using System;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Workspace
{
    public class ProjectFileChangeSet<TChange> : IProjectFileChangeSet<TChange> where TChange : class
    {
        readonly Dictionary<string, HashSet<TChange>> projectFiles = new Dictionary<string, HashSet<TChange>>();

        public IReadOnlyList<string> ProjectGuids => projectFiles.Keys.ToList();

        public bool ContainsProject(string projectGuid)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                return false;
            }

            return projectFiles.ContainsKey(projectGuid);
        }

        public void AddChange(string projectGuid, TChange change)
        {
            if (string.IsNullOrEmpty(projectGuid)
                || change is null)
            {
                return;
            }

            if (!projectFiles.ContainsKey(projectGuid))
            {
                projectFiles[projectGuid] = new HashSet<TChange>();
            }

            projectFiles[projectGuid].Add(change);
        }

        public IReadOnlyList<TChange> GetChanges(string projectGuid)
        {
            if (string.IsNullOrEmpty(projectGuid)
                || !projectFiles.ContainsKey(projectGuid))
            {
                return new List<TChange>();
            }

            return projectFiles[projectGuid].ToList();
        }
    }
}
