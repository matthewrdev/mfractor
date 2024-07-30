using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MFractor.Ide.Navigation;
using System.Linq;
using MFractor.Workspace;

namespace MFractor.Ide.Navigation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IRelationalNavigationService))]
    class RelationalNavigationService : IRelationalNavigationService
    {
        readonly Lazy<IRelationalNavigationHandlerRepository> relationalNavigationHandlers;
        public IRelationalNavigationHandlerRepository RelationalNavigationHandlerRepository => relationalNavigationHandlers.Value;

        [ImportingConstructor]
        public RelationalNavigationService(Lazy<IRelationalNavigationHandlerRepository> relationalNavigationHandlers)
        {
            this.relationalNavigationHandlers = relationalNavigationHandlers;
        }

        public IRelationalNavigationHandler ResolveRelationalNavigationHandler(IProjectFile projectFile)
        {
            if (projectFile is null)
            {
                return default;
            }

            return ResolveRelationalNavigationHandler(projectFile.CompilationProject, projectFile.FilePath);
        }

        public IRelationalNavigationHandler ResolveRelationalNavigationHandler(Project project, string filePath)
        {
            var availableHandlers = RelationalNavigationHandlerRepository.GetAvailableNavigationHandlers(project, filePath);

            if (availableHandlers is null || !availableHandlers.Any())
            {
                return default;
            }

            return availableHandlers.FirstOrDefault();
        }
    }
}