using System.Linq;
using MFractor.Data.Models;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    public class ClassDeclarationRepository : EntityRepository<ClassDeclaration>
    {
        public ClassDeclaration GetClassForFile(ProjectFile file)
        {
            return GetClassForFile(file.PrimaryKey);
        }

        public ClassDeclaration GetClassForFile(int projectFileId)
        {
            return Query(data => data.Values.FirstOrDefault(entity => entity.ProjectFileKey == projectFileId));
        }
    }
}
