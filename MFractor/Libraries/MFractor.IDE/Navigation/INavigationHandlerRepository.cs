using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Navigation
{
    public interface INavigationHandlerRepository : IPartRepository<INavigationHandler>
    {
        IReadOnlyList<INavigationHandler> NavigationHandlers { get; }

        INavigationHandler GetNavigationHandler(Type type);
    }
}