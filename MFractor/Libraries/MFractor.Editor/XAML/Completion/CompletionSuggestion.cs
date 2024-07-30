using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace MFractor.Editor.XAML.Completion
{
    [DebuggerDisplay("{DisplayText} - {Insertion}")]
    public class CompletionSuggestion : ICompletionSuggestion
    {
        public CompletionSuggestion()
        {
        }

        public CompletionSuggestion(string displayText, ImageElement icon)
        {
            DisplayText = displayText;
            Insertion = displayText;
            Icon = icon;
        }

        public CompletionSuggestion(string displayText, string insertion)
        {
            DisplayText = displayText;
            Insertion = insertion;
        }

        public CompletionSuggestion(string displayText, string insertion, ImageElement icon)
        {
            DisplayText = displayText;
            Insertion = insertion;
            Icon = icon;
        }

        readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public IReadOnlyDictionary<string, object> Properties => properties;

        public string DisplayText { get; }

        public string Insertion { get; }

        public ImageElement Icon { get; }

        public TextSpan ApplicableToSpan { get; }

        public CompletionSuggestion AddProperty(string key, object value)
        {
            properties.Add(key, value);
            return this;
        }

        public T GetProperty<T>(string key)
        {
            if (Properties.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)value;
                }
                catch
                {
                    return default;
                }
            }

            return default;
        }
    }
}
