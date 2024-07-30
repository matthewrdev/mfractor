namespace MFractor.Code.Analysis
{
    public class AnalyserSuppression
    {
        public AnalyserSuppression(string diagnosticId,
                                   string identifier)
        {
            DiagnosticId = diagnosticId;
            Identifier = identifier;
        }

        public string DiagnosticId { get; }

        public string Identifier { get; }
    }
}