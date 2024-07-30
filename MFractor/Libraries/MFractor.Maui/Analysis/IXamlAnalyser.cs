using System;
using System.Threading;
using MFractor.Text;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    public interface IXamlAnalyser : IDisposable
    {
        public event EventHandler<XamlAnalysisResultEventArgs> OnAnalysisCompleted;

        void Analyse(ITextProvider textProvider, string filePath, ProjectId projectId, CancellationToken token);
    }
}
