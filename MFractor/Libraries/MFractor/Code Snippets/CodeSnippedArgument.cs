using System;
using System.Diagnostics;
using MFractor.Utilities;

namespace MFractor.CodeSnippets
{
    [DebuggerDisplay("{Name} - '{Value}' - {Description}")]
    class CodeSnippetArgument : ICodeSnippetArgument
    {
        public string Name { get; }

        public string FormattedName => $"${Name}$";

        public string Value { get; set; }

        public string Description { get; }

        public int Order { get; }

        public CodeSnippetArgument(string name, 
                                   string value,
                                   string description = "",
                                   int order = 0)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            Name = Name.Replace("$", string.Empty).Trim();

            Value = value;
            Description = description;
            Order = order;
        }

        public CodeSnippetArgument(ReservedCodeSnippetArgumentName name,
                                   string value,
                                   string description = "",
                                   int order = 0)
        {
            Name = EnumHelper.GetEnumDescription(name);
            Value = value;
            Description = description;
            Order = order;
        }

        public string GetValue(EmptyCodeSnippetArgumentMode mode = EmptyCodeSnippetArgumentMode.Empty)
        {
            if (string.IsNullOrEmpty(Value))
            {
                switch (mode)
                {
                    case EmptyCodeSnippetArgumentMode.Empty:
                        return string.Empty;
                    case EmptyCodeSnippetArgumentMode.Name:
                        return FormattedName;
                    case EmptyCodeSnippetArgumentMode.NotSetPlaceholderValue:
                        return "$notset$";
                }
            }

            return Value;
        }
    }
}
