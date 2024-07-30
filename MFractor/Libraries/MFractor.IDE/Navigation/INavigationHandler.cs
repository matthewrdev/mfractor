using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Work;

namespace MFractor.Navigation
{
    [InheritedExport]
    public interface INavigationHandler
    {
        bool IsAvailable(INavigationContext navigationContext);

        Task<INavigationSuggestion> Suggest(INavigationContext navigationContext);

        Task<IReadOnlyList<IWorkUnit>> Navigate(INavigationContext navigationContext, INavigationSuggestion navigationSuggestion);
    }
}