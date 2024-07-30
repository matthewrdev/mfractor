using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Configuration;
using MFractor.Work;

namespace MFractor.Code.Scaffolding
{
    /// <summary>
    /// A scaffolder is a file generation component.
    /// </summary>
    [InheritedExport]
    public interface IScaffolder : IConfigurable, IAnalyticsFeature
    {
        /// <summary>
        /// The hierachical path that categorises this scaffolder.
        /// </summary>
        IReadOnlyList<string> Categorisation { get; }

        /// <summary>
        /// A short description of the criteria required for this scaffolder to provide scaffolding suggestions.
        /// </summary>
        string Criteria { get; }

        /// <summary>
        /// A URL to the help/documentation for this scaffolder.
        /// </summary>
        string HelpUrl { get; }

        /// <summary>
        /// An optional configuration for this scaffolder.
        /// <para/>
        /// Scaffolder configurations should use the <see cref="IScaffolderConfiguration.Configure(IScaffoldingContext)"/> method to launch a settings window
        /// </summary>
        IScaffolderConfiguration Configuration { get; }

        /// <summary>
        /// Is this scaffolder available in the <see cref="scaffoldingContext"/>?
        /// <para/>
        /// This method includes or discards this scaffolder from the current scaffolding session based on a cheap condition.
        /// </summary>
        /// <param name="scaffoldingContext"></param>
        /// <returns></returns>
        bool IsAvailable(IScaffoldingContext scaffoldingContext);

        /// <summary>
        /// Can this scaffolder provide the initial <see cref="IScaffoldingInput"/>?.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        bool CanSuggestInitialInput(IScaffoldingContext context, IScaffoldingInput input);

        /// <summary>
        /// Suggest the <see cref="IScaffoldingInput"/> that the scaffolding session should start with.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        IScaffoldingInput SuggestInitialInput(IScaffoldingContext context, IScaffoldingInput input);

        /// <summary>
        /// 
        /// </summary>
        IScaffolderState ProvideState(IScaffoldingContext context, IScaffoldingInput input);

        /// <summary>
        /// Can this scaffolder provide scaffolding suggestions given the <paramref name="context"/> and <paramref name="input"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state);

        /// <summary>
        /// Suggest a series of <see cref="IScaffoldingSuggestion"/>'s based on the <paramref name="context"/> and <paramref name="input"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state);

        /// <summary>
        /// Provide the <see cref="IWorkUnit"/>'s required to apply the <paramref name="suggestion"/> for the given <paramref name="context"/>, <paramref name="state"/> and <paramref name="input"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="suggestion"></param>
        /// <returns></returns>
        IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state, IScaffoldingSuggestion suggestion);
    }
}
