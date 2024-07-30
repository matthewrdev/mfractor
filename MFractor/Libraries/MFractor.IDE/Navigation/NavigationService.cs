using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Work;

namespace MFractor.Navigation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(INavigationService))]
    class NavigationService : INavigationService
    {
        readonly Lazy<INavigationHandlerRepository> navigationHandlers;
        public INavigationHandlerRepository NavigationHandlers => navigationHandlers.Value;

        [ImportingConstructor]
        public NavigationService(Lazy<INavigationHandlerRepository> navigationHandlers)
        {
            this.navigationHandlers = navigationHandlers;
        }

        public async Task<INavigationSuggestion> Suggest(INavigationContext navigationContext)
        {
            var availableHandlers = NavigationHandlers.Where(nh => nh.IsAvailable(navigationContext));

            foreach (var handler in availableHandlers)
            {
                var suggestion = await handler.Suggest(navigationContext);

                if (suggestion != null)
                {
                    return suggestion;
                }
            }

            return default;
        }

        public async Task<IReadOnlyList<IWorkUnit>> Navigate(INavigationContext navigationContext, INavigationSuggestion navigationSuggestion)
        {
            var handler = NavigationHandlers.GetNavigationHandler(navigationSuggestion.NavigationHandlerType);

            if (handler is null)
            {
                return Array.Empty<IWorkUnit>();
            }

            return await handler.Navigate(navigationContext, navigationSuggestion);
        }
    }
}