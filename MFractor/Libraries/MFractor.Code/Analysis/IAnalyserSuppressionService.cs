using System;
using System.Collections.Generic;
using MFractor.Code.Documents;

namespace MFractor.Code.Analysis
{
    public interface IAnalyserSuppressionService
    {
        /// <summary>
        /// Gets the suppressed analysers for the given document.
        /// <para/>
        /// Returns a dictionary of <see cref="AnalyserSuppression"/> where the key is the MFractor style identifier of the targetted analyser.
        /// <para/>
        /// Code analysers can be suppressed using the following syntax:
        /// <para/>
        /// <list type = "bullet">
        ///   <item>[MFractor: Suppress(MF1000)] where ''MF1000' is the Roslyn style diagnostic ID of the analyser.</item>
        ///   <item>[MFractor: Suppress(com.mfractor.identifier)] where 'com.mfractor.identifier' is the MFractor identifier of the analyser.</item>
        /// </list>
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        IReadOnlyDictionary<string, AnalyserSuppression> GetSuppressedAnalysers(IParsedXmlDocument xmlDocument);
    }
}
