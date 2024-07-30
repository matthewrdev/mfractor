using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MFractor.Documentation
{
    public interface IFeatureDocumentationService
    {
        IReadOnlyList<IFeatureDocumentation> Features { get; }

        IFeatureDocumentation GetFeatureDocumentationForDiagnostic(string diagnosticId);

        Task<bool> SynchroniseDocumentation(bool force);
    }
}