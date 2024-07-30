using System;

namespace MFractor.Code.Analysis
{
    public interface IIssue
    {
        string Message { get; }
        Type AnalyserType { get; }
        string DiagnosticId { get; }
        string Identifier { get; }
    }
}