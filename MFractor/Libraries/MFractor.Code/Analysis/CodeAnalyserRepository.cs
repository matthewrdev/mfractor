using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using MFractor.Xml;

namespace MFractor.Code.Analysis
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeAnalyserRepository))]
    sealed class CodeAnalyserRepository : PartRepository<IXmlSyntaxCodeAnalyser>, ICodeAnalyserRepository
    {
        readonly Lazy<IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>>> codeAnalysersByScope;
        IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>> CodeAnalysersByScope => codeAnalysersByScope.Value;

        readonly Lazy<IReadOnlyDictionary<string, IXmlSyntaxCodeAnalyser>> identifierIndexedCodeAnalysers;
        IReadOnlyDictionary<string, IXmlSyntaxCodeAnalyser> IdentifierIndexedCodeAnalysers => identifierIndexedCodeAnalysers.Value;

        readonly Lazy<IReadOnlyDictionary<string, IXmlSyntaxCodeAnalyser>> diagnosticIdIndexedCodeAnalysers;
        IReadOnlyDictionary<string, IXmlSyntaxCodeAnalyser> DiagnosticIdIndexedCodeAnalysers => diagnosticIdIndexedCodeAnalysers.Value;

        public IReadOnlyList<IXmlSyntaxCodeAnalyser> Analysers => Parts;

        [ImportingConstructor]
        public CodeAnalyserRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
            codeAnalysersByScope = new Lazy<IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>>>(() =>
            {
                return Analysers.GroupBy(ca => ca.Filter).ToDictionary(ca => ca.Key, group => (IReadOnlyList<IXmlSyntaxCodeAnalyser>)group.ToList());
            });

            identifierIndexedCodeAnalysers = new Lazy<IReadOnlyDictionary<string, IXmlSyntaxCodeAnalyser>>(() =>
            {
                return Analysers.ToDictionary(ca => ca.Identifier, ca => ca);
            });

            diagnosticIdIndexedCodeAnalysers = new Lazy<IReadOnlyDictionary<string, IXmlSyntaxCodeAnalyser>>(() =>
            {
                return Analysers.ToDictionary(ca => ca.DiagnosticId.ToUpper(), ca => ca);
            });
        }

        public IEnumerable<IXmlSyntaxCodeAnalyser> GetCodeAnalysersForSyntaxKind(XmlSyntaxKind type)
        {
            return Analysers.Where(r => r.TargetSyntax == type)
                                            .ToList();
        }

        public IEnumerable<IXmlSyntaxCodeAnalyser> GetCodeAnalysersForSyntaxKindAndScope(XmlSyntaxKind type, CodeAnalyserExecutionFilter scope)
        {
            if (scope == null)
            {
                return default;
            }

            if (!CodeAnalysersByScope.ContainsKey(scope))
            {
                return default;
            }

            return CodeAnalysersByScope[scope].Where(r => r.TargetSyntax == type)
                                              .ToList();
        }

        public IXmlSyntaxCodeAnalyser GetByIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return default;
            }

            if (IdentifierIndexedCodeAnalysers.TryGetValue(identifier, out var analyser))
            {
                return analyser;
            }

            return default;
        }

        public IXmlSyntaxCodeAnalyser GetByDiagnosticId(string diagnosticId)
        {
            if (string.IsNullOrEmpty(diagnosticId))
            {
                return default;
            }

            if (DiagnosticIdIndexedCodeAnalysers.TryGetValue(diagnosticId.ToUpper(), out var analyser))
            {
                return analyser;
            }

            return default;
        }
    }
}
