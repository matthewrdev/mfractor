using System;
using MFractor.Code.Scaffolding;

namespace MFractor.Views.Scaffolding
{
    public class ScaffoldingResult
    {
        public ScaffoldingResult(IScaffoldingContext scaffoldingContext,
                                 IScaffolder scaffolder,
                                 IScaffoldingInput scaffoldingInput,
                                 IScaffoldingSuggestion scaffoldingSuggestion)
        {
            ScaffoldingContext = scaffoldingContext;
            Scaffolder = scaffolder;
            ScaffoldingInput = scaffoldingInput;
            ScaffoldingSuggestion = scaffoldingSuggestion;
        }

        public IScaffoldingContext ScaffoldingContext { get; }

        public IScaffolder Scaffolder { get; }

        public IScaffoldingInput ScaffoldingInput { get; }

        public IScaffoldingSuggestion ScaffoldingSuggestion { get; }
    }
}
