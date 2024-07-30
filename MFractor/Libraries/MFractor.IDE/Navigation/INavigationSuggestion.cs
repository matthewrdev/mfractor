using System;
using System.Collections.Generic;
using MFractor.Work;

namespace MFractor.Navigation
{
    public interface INavigationSuggestion
    {
        Type NavigationHandlerType { get; }

        string Label { get; }

        string Description { get; }
    }
}