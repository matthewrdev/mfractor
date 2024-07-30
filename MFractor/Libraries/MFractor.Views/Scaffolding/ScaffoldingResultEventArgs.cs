using System;
using MFractor.Code.Scaffolding;

namespace MFractor.Views.Scaffolding
{
    public class ScaffoldingResultEventArgs : EventArgs
    {
        public ScaffoldingResultEventArgs(IScaffoldingContext scaffoldingContext,
                                          IScaffoldingInput scaffoldingInput,
                                          IScaffolder scaffolder,
                                          IScaffoldingSuggestion scaffoldingSuggestion,
                                          IScaffolderState scaffolderState)
        {
            ScaffoldingContext = scaffoldingContext ?? throw new ArgumentNullException(nameof(scaffoldingContext));
            ScaffoldingInput = scaffoldingInput ?? throw new ArgumentNullException(nameof(scaffoldingInput));
            Scaffolder = scaffolder ?? throw new ArgumentNullException(nameof(scaffolder));
            ScaffoldingSuggestion = scaffoldingSuggestion ?? throw new ArgumentNullException(nameof(scaffoldingSuggestion));
            ScaffolderState = scaffolderState ?? throw new ArgumentNullException(nameof(scaffolderState));
        }

        public IScaffoldingContext ScaffoldingContext { get; }

        public IScaffoldingInput ScaffoldingInput { get; }

        public IScaffolder Scaffolder { get; }

        public IScaffoldingSuggestion ScaffoldingSuggestion { get; }

        public IScaffolderState ScaffolderState { get; }
    }
}
