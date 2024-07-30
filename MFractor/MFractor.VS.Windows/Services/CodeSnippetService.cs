using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.CodeSnippets;
using MFractor.Configuration;
using Microsoft.CodeAnalysis;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeSnippetService))]
    class CodeSnippetService : ICodeSnippetService
    {
        readonly Lazy<IConfigurationEngine> configurationEngine;

        IConfigurationEngine ConfigurationEngine => configurationEngine.Value;

        readonly Lazy<ICodeSnippetFactory> codeSnippetFactory;
        public ICodeSnippetFactory CodeSnippetFactory => codeSnippetFactory.Value;

        public IEnumerable<ICodeSnippet> Snippets => GetIdeCodeSnippets();

        [ImportingConstructor]
        public CodeSnippetService(Lazy<IConfigurationEngine> configurationEngine,
                                  Lazy<ICodeSnippetFactory> codeSnippetFactory)
        {
            this.configurationEngine = configurationEngine;
            this.codeSnippetFactory = codeSnippetFactory;
        }

        public ICodeSnippet GetCodeSnippetById(string id)
        {
            return default;
        }

        internal IEnumerable<ICodeSnippet> GetIdeCodeSnippets()
        {
            // 
            return Enumerable.Empty<ICodeSnippet>();
        }

        public ICodeSnippet GetCodeSnippetFromProjectFile(Project project, string relativeFilePath)
        {
            var fi = new FileInfo(project.FilePath);

            var projectPath = Path.Combine(fi.Directory.FullName, relativeFilePath);

            var code = File.ReadAllText(projectPath);

            var name = Path.GetFileNameWithoutExtension(projectPath);
            var description = $"The code snippet {relativeFilePath} loaded from the project {project.Name}";
            var arguments = CodeSnippetFactory.ExtractArguments(code);

            return CodeSnippetFactory.CreateSnippet(name, description, code, arguments);
        }
    }
}
