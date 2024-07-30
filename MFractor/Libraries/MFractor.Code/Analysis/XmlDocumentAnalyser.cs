using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Code.Documents;
using MFractor.Xml;

namespace MFractor.Code.Analysis
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXmlDocumentAnalyser))]
    class XmlDocumentAnalyser : IXmlDocumentAnalyser
    {
        [ImportingConstructor]
        public XmlDocumentAnalyser(Lazy<ICodeAnalyserRepository> codeAnalyserRepository,
                                  Lazy<ICodeAnalysisOptions> codeAnalysisOptions,
                                  Lazy<ICodeAnalysisPreprocessorProviderRepository> codeAnalysisPreprocessorProviderRepository,
                                  Lazy<IAnalyserSuppressionService> analyserSuppressionService)
        {
            this.codeAnalyserRepository = codeAnalyserRepository;
            this.codeAnalysisOptions = codeAnalysisOptions;
            this.codeAnalysisPreprocessorProviderRepository = codeAnalysisPreprocessorProviderRepository;
            this.analyserSuppressionService = analyserSuppressionService;
        }

        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<ICodeAnalyserRepository> codeAnalyserRepository;
        ICodeAnalyserRepository CodeAnalyserRepository => codeAnalyserRepository.Value;

        readonly Lazy<ICodeAnalysisOptions> codeAnalysisOptions;
        ICodeAnalysisOptions CodeAnalysisOptions => codeAnalysisOptions.Value;

        readonly Lazy<ICodeAnalysisPreprocessorProviderRepository> codeAnalysisPreprocessorProviderRepository;
        public ICodeAnalysisPreprocessorProviderRepository CodeAnalysisPreprocessorProviderRepository => codeAnalysisPreprocessorProviderRepository.Value;

        readonly Lazy<IAnalyserSuppressionService> analyserSuppressionService;
        public IAnalyserSuppressionService AnalyserSuppressionService => analyserSuppressionService.Value;

        public IReadOnlyList<ICodeIssue> Analyse(IParsedXmlDocument document,
                                               IFeatureContext context,
                                               CancellationToken cancellation)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var suppressedAnalysers = AnalyserSuppressionService.GetSuppressedAnalysers(document);

            var nodeAnalysers = CodeAnalyserRepository.GetCodeAnalysersForSyntaxKind(XmlSyntaxKind.Node)
                                                      ?.Where(a => CodeAnalysisOptions.IsEnabled(a.Identifier))
                                                      ?.Where(a => a.IsInterestedInDocument(document, context))
                                                      ?.Where(a => !suppressedAnalysers.ContainsKey((a.Identifier)))
                                                      ?.GroupBy(a => a.Filter, a => a)
                                                      ?.ToDictionary(g => g.Key, g => (IReadOnlyList<IXmlSyntaxCodeAnalyser>)g.ToList());

            var attributeAnalysers = CodeAnalyserRepository.GetCodeAnalysersForSyntaxKind(XmlSyntaxKind.Attribute)
                                                      ?.Where(a => CodeAnalysisOptions.IsEnabled(a.Identifier))
                                                      ?.Where(a => a.IsInterestedInDocument(document, context))
                                                      ?.Where(a => !suppressedAnalysers.ContainsKey((a.Identifier)))
                                                      ?.GroupBy(a => a.Filter, a => a)
                                                      ?.ToDictionary(g => g.Key, g => (IReadOnlyList<IXmlSyntaxCodeAnalyser>)g.ToList());

            Preprocess(document, context);

            return Analyse(document, context, document.GetSyntaxTree().Root, nodeAnalysers, attributeAnalysers, cancellation);
        }

        void Preprocess(IParsedXmlDocument document, IFeatureContext context)
        {
            if (context.HasKey("IsPreprocessed"))
            {
                return;
            }

            var preprocessors = new List<ICodeAnalysisPreprocessor>();

            foreach (var provider in CodeAnalysisPreprocessorProviderRepository)
            {
                try
                {
                    var results = provider.ProvidePreprocessors(document, context);

                    if (results != null && results.Any())
                    {
                        preprocessors.AddRange(results.Where(r => r != null));
                    }
                }
                catch (Exception ex)
                {
                    log.Info($"{$"The pre-processor provider '"}{provider}' encountered an issue. {ex.ToString()}");
                }
            }

            if (preprocessors.Any())
            {
                foreach (var preprocessor in preprocessors)
                {

                    try
                    {
                        if (preprocessor.Preprocess(document, context))
                        {
                            context.Add(preprocessor.GetType().FullName, preprocessor);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Info($"{$"The pre-processor "}{preprocessor} encountered an issue. {ex}");

                    }
                }
            }

            context.Add("IsPreprocessed", true);
        }

        public IReadOnlyList<ICodeIssue> Analyse(IParsedXmlDocument document,
                                               IFeatureContext context,
                                               IReadOnlyList<IXmlSyntaxCodeAnalyser> analysers,
                                               CancellationToken cancellation)
        {

            var nodeAnalysers = analysers.Where(a => a.TargetSyntax == XmlSyntaxKind.Node)
                                         ?.Where(a => CodeAnalysisOptions.IsEnabled(a.Identifier))
                                         ?.Where(a => a.IsInterestedInDocument(document, context))
                                         ?.GroupBy(a => a.Filter, a => a)
                                         ?.ToDictionary(a => a.Key, a => (IReadOnlyList<IXmlSyntaxCodeAnalyser>)a.ToList());

            var attributeAnalysers = analysers.Where(a => a.TargetSyntax == XmlSyntaxKind.Attribute)
                                               ?.Where(a => CodeAnalysisOptions.IsEnabled(a.Identifier))
                                               ?.Where(a => a.IsInterestedInDocument(document, context))
                                               ?.GroupBy(a => a.Filter, a => a)
                                               ?.ToDictionary(a => a.Key, a => (IReadOnlyList<IXmlSyntaxCodeAnalyser>)a.ToList());

            Preprocess(document, context);

            return Analyse(document, context, document.GetSyntaxTree().Root, nodeAnalysers, attributeAnalysers, cancellation);
        }

        public IReadOnlyList<ICodeIssue> Analyse(IParsedXmlDocument document,
                                               IFeatureContext context,
                                               XmlNode nodeSyntax,
                                               IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>> nodeAnalysers,
                                               IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>> attributeAnalysers,
                                               CancellationToken cancellation)
        {
            var results = new List<ICodeIssue>();

            // Check that the node is in a valid state...
            if (!nodeSyntax.IsSelfClosing
                && !nodeSyntax.HasClosingTag)
            {
                return null;
            }

            if (nodeAnalysers != null && nodeAnalysers.Any())
            {
                AnalyseNode(document, context, nodeSyntax, nodeAnalysers, results, cancellation);
            }

            if (nodeSyntax.HasChildren)
            {
                AnalyseNodeChildren(document, context, nodeSyntax, nodeAnalysers, attributeAnalysers, results, cancellation);
            }

            if (nodeSyntax.HasAttributes
                && attributeAnalysers != null
                && attributeAnalysers.Any())
            {
                AnalyseAttributes(document, context, nodeSyntax, attributeAnalysers, results, cancellation);
            }

            return results;
        }

        void AnalyseNode(IParsedXmlDocument document,
                         IFeatureContext context,
                         XmlNode nodeSyntax,
                         IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>> nodeAnalysers,
                         List<ICodeIssue> results, CancellationToken cancellation)
        {
            foreach (var filterGroup in nodeAnalysers)
            {
                cancellation.ThrowIfCancellationRequested();

                var filter = filterGroup.Key;

                if (filter.Filter != null
                    && !filter.Filter(context, nodeSyntax))
                {
                    continue;
                }

                foreach (var analyser in filterGroup.Value)
                {
                    IEnumerable<ICodeIssue> issues = null;
                    try
                    {
                        issues = analyser.AnalyseSyntax(nodeSyntax, document, context);
                    }
                    catch (OperationCanceledException oex)
                    {
                        throw oex;
                    }
                    catch (Exception ex)
                    {
                        log?.Warning(analyser.Name + " threw an exception while analysing '" + nodeSyntax.Name + "'. Error:\n" + ex.ToString());
                        continue;
                    }

                    if (issues != null
                        && issues.Any())
                    {
                        results.AddRange(issues);
                    }
                }
            }
        }

        void AnalyseAttributes(IParsedXmlDocument document,
                               IFeatureContext context,
                               XmlNode nodeSyntax,
                               IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>> attributeAnalysers,
                               List<ICodeIssue> results, CancellationToken cancellation)
        {
            foreach (var attributeSyntax in nodeSyntax.Attributes)
            {
                if (attributeSyntax == null)
                {
                    log.Info($"A null attribute was detected in {attributeSyntax.Name}. Skipping.");
                    continue;
                }
                cancellation.ThrowIfCancellationRequested();

                foreach (var filterGroup in attributeAnalysers)
                {
                    cancellation.ThrowIfCancellationRequested();

                    var filter = filterGroup.Key;

                    if (filter.Filter != null
                        && !filter.Filter(context, attributeSyntax))
                    {
                        continue;
                    }

                    foreach (var analyser in filterGroup.Value)
                    {
                        IEnumerable<ICodeIssue> issues = null;
                        try
                        {
                            issues = analyser.AnalyseSyntax(attributeSyntax, document, context);
                        }
                        catch (OperationCanceledException oex)
                        {
                            throw oex;
                        }
                        catch (Exception ex)
                        {
                            log?.Warning(analyser.Name + " threw an exception while analysing '" + attributeSyntax.Name + "'. Error:\n" + ex.ToString());
                            continue;
                        }

                        if (issues != null && issues.Any())
                        {
                            results.AddRange(issues);
                        }
                    }
                }
            }
        }

        void AnalyseNodeChildren(IParsedXmlDocument document,
                                 IFeatureContext context,
                                 XmlNode nodeSyntax,
                                 IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>> nodeAnalysers,
                                 IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>> attributeAnalysers,
                                 List<ICodeIssue> results,
                                 CancellationToken cancellation)
        {
            foreach (var el in nodeSyntax.Children)
            {
                if (el == null)
                {
                    log.Info($"A null child node was detected in {nodeSyntax.Name.FullName}. Skipping.");
                    continue;
                }

                cancellation.ThrowIfCancellationRequested();
                try
                {
                    var issues = Analyse(document, context, el, nodeAnalysers, attributeAnalysers, cancellation);

                    if (issues != null && issues.Any())
                    {
                        results.AddRange(issues);
                    }
                }
                catch (OperationCanceledException oex)
                {
                    throw oex;
                }
                catch (Exception ex)
                {
                    log?.Warning(nodeSyntax.ToString());
                    log?.Warning(ex.ToString());
                }
            }
        }
    }
}

