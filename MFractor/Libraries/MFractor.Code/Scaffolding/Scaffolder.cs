using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration.Options;
using MFractor.Configuration;
using MFractor.Work;

namespace MFractor.Code.Scaffolding
{
    public abstract class Scaffolder : Configurable, IScaffolder
    {
        public abstract string AnalyticsEvent { get; }

        public abstract string Criteria { get; }

        public virtual IReadOnlyList<string> Categorisation { get; } = new List<string>();

        public virtual string HelpUrl { get; } = string.Empty;

        public virtual IScaffolderConfiguration Configuration { get; }

        public virtual IScaffolderState ProvideState(IScaffoldingContext context, IScaffoldingInput input)
        {
            return ScaffolderState.Empty;
        }

        public abstract bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state);

        public abstract IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state);

        public abstract IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state, IScaffoldingSuggestion suggestion);

        protected IScaffoldingSuggestion CreateSuggestion(string description,
                                                          int priority = 0,
                                                          Enum actionId = default,
                                                          ICodeGenerationOptionSet options = null,
                                                          params IScaffoldingMatch[] matches)
        {
            return new ScaffoldingSuggestion(matches, Name, description, Identifier, priority, options, Convert.ToInt32(actionId));
        }

        public virtual bool IsAvailable(IScaffoldingContext context)
        {
            return true;
        }

        public virtual bool CanSuggestInitialInput(IScaffoldingContext context, IScaffoldingInput input)
        {
            return false;
        }

        public virtual IScaffoldingInput SuggestInitialInput(IScaffoldingContext context, IScaffoldingInput input)
        {
            return default;
        }
    }
}
