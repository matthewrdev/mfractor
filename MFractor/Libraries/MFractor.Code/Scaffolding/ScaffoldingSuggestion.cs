using System;
using System.Collections.Generic;
using MFractor.Code.Scaffolding;
using MFractor.Code.CodeGeneration.Options;

namespace MFractor.Code.Scaffolding
{
    public class ScaffoldingSuggestion : IScaffoldingSuggestion
    {
        public ScaffoldingSuggestion(IReadOnlyList<IScaffoldingMatch> matches,
                                     string name,
                                     string description,
                                     string scaffolderId,
                                     int priority,
                                     ICodeGenerationOptionSet options,
                                     int actionId = 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("message", nameof(name));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("message", nameof(description));
            }

            if (string.IsNullOrEmpty(scaffolderId))
            {
                throw new ArgumentException("message", nameof(scaffolderId));
            }

            Matches = matches ?? new List<IScaffoldingMatch>();
            Name = name;
            Description = description;
            ScaffolderId = scaffolderId;
            Priority = priority;
            Options = options;
            ActionId = actionId;
        }

        public IReadOnlyList<IScaffoldingMatch> Matches { get; }

        public string Name { get; }

        public string Description { get; }

        public int Priority { get; }

        public ICodeGenerationOptionSet Options { get; }

        public string ScaffolderId { get; }

        public int ActionId { get; }

        public bool IsAction(Enum action)
        {
            return ActionId == Convert.ToInt32(action);
        }

        public TEnum GetAction<TEnum>() where TEnum : Enum
        {
            return (TEnum)((object)ActionId);
        }
    }
}