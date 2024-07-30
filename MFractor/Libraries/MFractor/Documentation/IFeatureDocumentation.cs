using System;

namespace MFractor.Documentation
{
    public interface IFeatureDocumentation
    {
        int FeatureId { get; }

        string DiagnosticId { get; }

        string FeatureName { get; }

        string Url { get; }

        bool HasUrl { get; }
    }
}