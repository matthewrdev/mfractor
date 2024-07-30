using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Models;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    /// <summary>
    /// The repository that provides data-access for the <see cref="AutomationIdDeclaration"/> table.
    /// </summary>
    public class AutomationIdDeclarationRepository: EntityRepository<AutomationIdDeclaration>
    {
        public IReadOnlyList<AutomationIdDeclaration> GetAutomationIdsInFile(ProjectFile file)
        {
            return GetAutomationIdsInFile(file.PrimaryKey);
        }

        public IReadOnlyList<AutomationIdDeclaration> GetAutomationIdsInFile(int fileKey)
        {
            return Query(d => d.Values.Where(ad => ad.ProjectFileKey == fileKey && !ad.GCMarked).ToList());
        }

        public AutomationIdDeclaration GetNamedAutomationIdInFile(string name, ProjectFile file)
        {
            return GetNamedAutomationIdInFile(name, file.PrimaryKey);
        }

        public AutomationIdDeclaration GetNamedAutomationIdInFile(string name, int fileKey)
        {
            return Query(d => d.Values.FirstOrDefault(ad => ad.Name == name && ad.ProjectFileKey == fileKey && !ad.GCMarked));
        }

        public IReadOnlyList<AutomationIdDeclaration> GetAllNamedAutomationIdInFile(string name, ProjectFile file)
        {
            return GetAllNamedAutomationIdInFile(name, file.PrimaryKey);
        }

        public IReadOnlyList<AutomationIdDeclaration> GetAllNamedAutomationIdInFile(string name, int fileKey)
        {
            return Query(d => d.Values.Where(ad => ad.Name == name && ad.ProjectFileKey == fileKey && !ad.GCMarked).ToList());
        }
    }
}
