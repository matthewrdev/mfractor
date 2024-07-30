using System;
using Microsoft.CodeAnalysis.Options;

namespace MFractor.Code.Formatting
{
    public interface ICSharpFormattingPolicy
    {
        OptionSet OptionSet { get; }
    }
}
