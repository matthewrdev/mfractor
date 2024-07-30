using System;

namespace MFractor.Navigation
{
    class NavigationSuggestion : INavigationSuggestion
    {
        public NavigationSuggestion(string label,
                                    string description,
                                    Type navigationHandlerType)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            Label = label;
            Description = description ?? string.Empty;
            NavigationHandlerType = navigationHandlerType ?? throw new ArgumentNullException(nameof(navigationHandlerType));
        }

        public NavigationSuggestion(string label,
                                    string description,
                                    INavigationHandler navigationHandler)
            :this(label, description, navigationHandler.GetType())
        {
        }

        public string Label { get; }

        public string Description { get; }

        public Type NavigationHandlerType { get; }
    }
}