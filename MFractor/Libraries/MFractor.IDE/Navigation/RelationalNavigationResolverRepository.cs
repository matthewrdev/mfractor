using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.Navigation
{
    [Export(typeof(IRelationalNavigationHandlerRepository))]
    class RelationalNavigationHandlerRepository : PartRepository<IRelationalNavigationHandler>, IRelationalNavigationHandlerRepository
    {
        [ImportingConstructor]
        public RelationalNavigationHandlerRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IRelationalNavigationHandler> RelationalNavigationHandlers => Parts;

        public IEnumerable<IRelationalNavigationHandler> GetAvailableNavigationHandlers(Project project, string filePath)
        {
            return RelationalNavigationHandlers.Where(handler => handler.IsAvailable(project, filePath));
        }
    }
}