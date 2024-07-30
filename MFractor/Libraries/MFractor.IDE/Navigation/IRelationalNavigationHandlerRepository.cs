using System;
using System.Collections.Generic;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.Navigation
{
    public interface IRelationalNavigationHandlerRepository : IPartRepository<IRelationalNavigationHandler>
    {
        IReadOnlyList<IRelationalNavigationHandler> RelationalNavigationHandlers { get; }

        IEnumerable<IRelationalNavigationHandler> GetAvailableNavigationHandlers(Project project, string filePath);
    }
}