using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Work;

namespace MFractor.Navigation
{
    public interface INavigationService
    {
        Task<INavigationSuggestion> Suggest(INavigationContext navigationContext);

        Task<IReadOnlyList<IWorkUnit>> Navigate(INavigationContext navigationContext, INavigationSuggestion navigationSuggestion);
    }
}