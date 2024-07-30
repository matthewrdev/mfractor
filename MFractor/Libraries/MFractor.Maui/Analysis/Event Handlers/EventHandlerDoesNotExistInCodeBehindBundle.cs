using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.EventHandlers
{
    class EventHandlerDoesNotExistInCodeBehindBundle
    {
        public EventHandlerDoesNotExistInCodeBehindBundle(IEventSymbol eventSymbol, IEnumerable<IMethodSymbol> callbacks)
        {
            EventSymbol = eventSymbol;
            Callbacks = (callbacks ?? Enumerable.Empty<IMethodSymbol>()).ToList();
        }

        public IEventSymbol EventSymbol { get; }

        public IReadOnlyList<IMethodSymbol> Callbacks { get; }
    }
}

