using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace MFractor.Editor.XAML.Completion
{
    public interface ICompletionSuggestion
    {
        IReadOnlyDictionary<string, object> Properties { get; } 

        string DisplayText { get; }

        string Insertion { get; }

        ImageElement Icon { get; }

        TextSpan ApplicableToSpan { get; }

        T GetProperty<T>(string key);
    }
}
