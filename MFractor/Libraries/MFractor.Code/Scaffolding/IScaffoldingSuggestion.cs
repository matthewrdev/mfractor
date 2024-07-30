using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration.Options;

namespace MFractor.Code.Scaffolding
{
    public interface IScaffoldingSuggestion
    {
        IReadOnlyList<IScaffoldingMatch> Matches { get; }

        string Name { get; }

        string Description { get; }

        int Priority { get; }

        ICodeGenerationOptionSet Options { get; }

        string ScaffolderId { get; }

        /// <summary>
        /// When an <see cref="ICodeAction"/> returns multiple suggestions, a unique id of the action to execute.
        /// </summary>
        /// <value>The action identifier.</value>
        int ActionId { get; }

        /// <summary>
        /// Gets the <see cref="ActionId"/> cast as <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        TEnum GetAction<TEnum>() where TEnum : Enum;

        /// <summary>
        /// Is the code action suggestion the provided <paramref name="action"/>?
        /// </summary>
        /// <returns><c>true</c>, if action was ised, <c>false</c> otherwise.</returns>
        /// <param name="action">Action.</param>
        bool IsAction(Enum action);
    }
}
