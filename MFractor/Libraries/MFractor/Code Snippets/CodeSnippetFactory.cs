using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MFractor.CodeSnippets.Exceptions;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using MFractor.Logging;
using MFractor.Utilities;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.CodeSnippets
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeSnippetFactory))]
    class CodeSnippetFactory : ICodeSnippetFactory
    {
        readonly ILogger log = Logger.Create();

        public Regex ArgumentsRegex { get; } = new Regex("\\$[^$]*\\$", RegexOptions.Compiled);

        public IReadOnlyList<ICodeSnippetArgument> ExtractArguments(string code)
        {
            var values = new List<string>();

            foreach (Match m in ArgumentsRegex.Matches(code))
            {
                var value = m.Value.Substring(1, m.Value.Length - 2);
                if (!values.Contains(value))
                {
                    values.Add(value);
                }
            }

            return values.Select(v => CreateArgument(v, string.Empty, v)).ToList();
        }

        public IReadOnlyList<TextSpan> ExtractArgumentSpans(string code)
        {
            var values = new List<TextSpan>();

            foreach (Match m in ArgumentsRegex.Matches(code))
            {
                values.Add(TextSpan.FromBounds(m.Index, m.Index + m.Length));
            }

            return values;
        }

        public ICodeSnippet CreateSnippet(string name, string description, string code)
        {
            return CreateSnippet(name, description, code, Array.Empty<ICodeSnippetArgument>());
        }

        public ICodeSnippet CreateSnippet(string name,
                                          string description,
                                          string code,
                                          IReadOnlyList<ICodeSnippetArgument> arguments)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The code snippet name cannot be null or empty", nameof(name));
            }

            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("The code snippet cannot be null or empty", nameof(code));
            }

            arguments = arguments ?? Array.Empty<ICodeSnippetArgument>();

            var snippet = new CodeSnippet(name, description, code, arguments);

            return snippet;
        }

        public ICodeSnippet CreateSnippet(string name,
                                          string description,
                                          string code,
                                          IReadOnlyDictionary<string, string> arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            var args = arguments.Select(kv => CreateArgument(kv.Key, kv.Value)).ToList();

            return CreateSnippet(name, description, code, args);
        }

        public ICodeSnippetArgument CreateArgument(string name, string value = "", string description = "", int order = 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The code snippet argument name cannot be null or empty", nameof(name));
            }

            return new CodeSnippetArgument(name, value, description, order);
        }

        public ICodeSnippet CreateSnippetFromEmbeddedResource(Assembly assembly, string snippetName, IReadOnlyList<ICodeSnippetArgument> arguments = null)
        {
            if (assembly == null || string.IsNullOrEmpty(snippetName))
            {
                return default;
            }

            var snippetResourceName = snippetName.Replace("\\", ".").Replace("/", ".");

            var resourceId = ResourcesHelper.LocateMatchingResourceId(assembly, snippetResourceName);

            if (string.IsNullOrEmpty(resourceId))
            {
                throw new CodeSnippetResourceNotFoundException(snippetName, assembly);
            }

            var code = ResourcesHelper.ReadResourceContent(assembly, resourceId);

            if (string.IsNullOrEmpty(code))
            {
                log?.Warning("The default code snippet for " + snippetResourceName + " is empty!");
                Debugger.Break();
            }

            if (arguments is null)
            {
                arguments = ExtractArguments(code);
            }
            var name = Path.GetFileNameWithoutExtension(snippetName);
            var description = "(MFractor) - The default code snippet for " + Path.GetFileName(snippetName);

            return CreateSnippet(name, description, code, arguments);
        }

        public ICodeSnippet CreateSnippetFromValue(string snippetName, string code)
        {
            var arguments = ExtractArguments(code);
            var description = "(MFractor) - The default code snippet for " + snippetName;

            return CreateSnippet(snippetName, description, code, arguments);
        }

        /// <summary>
        /// Gets the default snippet for a configurable property name. This method is guaranteed to return a code snippet.
        /// <para/>
        /// If the code snippet resource could not be found, this method will throw a CodeSnippetResourceNotFoundException.
        /// </summary>
        /// <returns>The default snippet for property.</returns>
        /// <param name="configurable">Configurable.</param>
        /// <param name="name">The name of .</param>
        public ICodeSnippet GetDefaultSnippetForProperty(IConfigurable configurable, string name)
        {
            var property = configurable.GetConfigurableProperty(name);

            if (property == null)
            {
                return null;
            }

            return GetDefaultSnippetForProperty(configurable, property);
        }

        /// <summary>
        /// Gets the default snippet for a configurable property. This method is guaranteed to return a code snippet.
        /// <para/>
        /// If the code snippet resource could not be found, this method will throw a CodeSnippetResourceNotFoundException.
        /// </summary>
        /// <returns>The default snippet for property.</returns>
        /// <param name="configurable">Configurable.</param>
        /// <param name="property">Property.</param>
        public ICodeSnippet GetDefaultSnippetForProperty(IConfigurable configurable, IConfigurableProperty property)
        {
            var defaultCodeResourceAttribute = property.Property.GetCustomAttribute<CodeSnippetResourceAttribute>();

            if (defaultCodeResourceAttribute == null)
            {
                var defaultSnippet = property.Property.GetCustomAttribute<CodeSnippetDefaultValueAttribute>();

                if (defaultSnippet != null)
                {
                    return CreateSnippetFromValue(defaultSnippet.Description, defaultSnippet.Code);
                }

                return null;
            }

            List<ICodeSnippetArgument> arguments = null;

            var argumentAttributes = property.Property.GetCustomAttributes<CodeSnippetArgumentAttribute>();
            if (argumentAttributes != null && argumentAttributes.Any())
            {
                arguments = argumentAttributes.Select(a => new CodeSnippetArgument(a.Name, value: null, a.Description, a.Order)).OfType<ICodeSnippetArgument>().ToList();
            }

            return CreateSnippetFromEmbeddedResource(configurable.GetType().Assembly, defaultCodeResourceAttribute.CodeSnippetFilePath, arguments);
        }
    }
}
