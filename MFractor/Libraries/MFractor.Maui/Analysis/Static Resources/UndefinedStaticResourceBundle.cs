using System.Collections.Generic;
using MFractor.Maui.Syntax.Expressions;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.StaticResources
{
    class UndefinedStaticResourceBundle
    {
        public UndefinedStaticResourceBundle(StaticResourceExpression staticResourceExpression,
                                             IReadOnlyList<INamedTypeSymbol> availableResourceDictionaries,
                                             string suggestedStaticResource)
        {
            StaticResourceExpression = staticResourceExpression;
            AvailableResourceDictionaries = availableResourceDictionaries;
            SuggestedStaticResource = suggestedStaticResource;
        }

        public StaticResourceExpression StaticResourceExpression { get; }
        public IReadOnlyList<INamedTypeSymbol> AvailableResourceDictionaries { get; }

        public string SuggestedStaticResource { get; }
    }
}