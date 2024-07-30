using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Work;

namespace MFractor.Navigation
{
    public abstract class NavigationHandler : INavigationHandler
    {
        public abstract bool IsAvailable(INavigationContext navigationContext);

        public abstract Task<IReadOnlyList<IWorkUnit>> Navigate(INavigationContext navigationContext, INavigationSuggestion navigationSuggestion);

        public abstract Task<INavigationSuggestion> Suggest(INavigationContext navigationContext);

        protected INavigationSuggestion CreateSuggestion(string label, string description = "")
        {
            return new NavigationSuggestion(label, description, this);
        }
    }
}