using System;
using System.Diagnostics;

namespace MFractor.Code.CodeActions
{
    [DebuggerDisplay("{Description}")]
    class CodeActionSuggestion : ICodeActionSuggestion
    {
        public string Description { get; }

        public string CodeActionIdentifier { get; }

        public int ActionId { get; }

        public CodeActionSuggestion(string description,
                                    string codeActionIdentifier,
                                    int actionId)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("message", nameof(description));
            }

            if (string.IsNullOrEmpty(codeActionIdentifier))
            {
                throw new ArgumentException("message", nameof(codeActionIdentifier));
            }

            Description = description;
            CodeActionIdentifier = codeActionIdentifier;
            ActionId = actionId;
        }

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
