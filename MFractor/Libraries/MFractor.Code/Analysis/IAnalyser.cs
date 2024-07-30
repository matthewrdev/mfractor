using System;
using MFractor.Configuration;

namespace MFractor.Code.Analysis
{
    public interface IAnalyser : IConfigurable
    {
        IssueClassification IssueClassification { get; }

        string DiagnosticId { get; }
    }
}