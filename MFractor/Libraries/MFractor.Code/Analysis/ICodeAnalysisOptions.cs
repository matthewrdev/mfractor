using System;
using System.Collections.Generic;

namespace MFractor.Code.Analysis
{
    public interface ICodeAnalysisOptions
    {
        void ToggleAnalyser(IXmlSyntaxCodeAnalyser codeAnalyser, bool enable);
        void ToggleAnalyser<TCodeAnalyser>(bool enable) where TCodeAnalyser : class, IXmlSyntaxCodeAnalyser;
        void ToggleAnalyser(string identifier, bool enable);

        void ToggleAnalysers(IReadOnlyDictionary<IXmlSyntaxCodeAnalyser, bool> changes);
        void ToggleAnalysers(IReadOnlyDictionary<string, bool> changes);

        bool IsEnabled(IXmlSyntaxCodeAnalyser codeAnalyser, bool bypassCache = false);
        bool IsEnabled<TCodeAnalyser>(bool bypassCache = false) where TCodeAnalyser : class, IXmlSyntaxCodeAnalyser;
        bool IsEnabled(string identifier, bool bypassCache = false);

        string GetCodeAnalyserKey(IXmlSyntaxCodeAnalyser codeAnalyser);
        string GetCodeAnalyserKey<TCodeAnalyser>() where TCodeAnalyser : class, IXmlSyntaxCodeAnalyser;
        string GetCodeAnalyserKey(string identifier);

        event EventHandler<CodeAnalysisStateChangedEventArgs> CodeAnalysisStateChanged;
    }
}
