using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Utilities;

namespace MFractor.CodeSnippets
{
    class CodeSnippet : ICodeSnippet
    {
        readonly List<ICodeSnippetArgument> arguments = new List<ICodeSnippetArgument>();
        public IReadOnlyList<ICodeSnippetArgument> Arguments => arguments;

        public string Name { get; }

        public string Description { get; }

        public string TemplatedCode { get; }

        public CodeSnippet(string name, string description, string code)
            : this (name, description, code, Array.Empty<ICodeSnippetArgument>())
        {
        }

        public CodeSnippet(string name, 
                           string description, 
                           string code,
                           IReadOnlyList<ICodeSnippetArgument> arguments)
        {
            Name = name;
            Description = description;
            TemplatedCode = code;
            this.arguments = (arguments?.ToList() ?? Array.Empty<ICodeSnippetArgument>().ToList()).OrderBy(a => a.Order).ThenBy(a => a.Name).ToList();
        }

        public string GetFormattedCode(EmptyCodeSnippetArgumentMode mode)
        {
            var code = TemplatedCode;

            if (Arguments != null && Arguments.Any())
            {
                foreach (var arg in Arguments)
                {
                    code = code.Replace(arg.FormattedName, arg.GetValue(mode));
                }
            }

            return code;
        }

        public override string ToString()
        {
            return GetFormattedCode(EmptyCodeSnippetArgumentMode.Name);
        }

        public ICodeSnippetArgument GetNamedArgument(string name)
        {
            return Arguments.FirstOrDefault(arg => arg.Name == name);
        }

        public ICodeSnippet SetArgumentValue(string name, string value)
        {
            var argument = GetNamedArgument(name);

            if (argument == null)
            {
                argument = new CodeSnippetArgument(name, value);
                arguments.Add(argument);
            } 
            else 
            {
                argument.Value = value;
            }

            return this;
        }

        public ICodeSnippet SetArgumentValue(ReservedCodeSnippetArgumentName name, string value)
        {
            var argumentName = EnumHelper.GetEnumDescription(name);

            SetArgumentValue(argumentName, value);

            return this;
        }
    }
}
