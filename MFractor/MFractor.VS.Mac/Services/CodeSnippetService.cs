using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.CodeSnippets;
using Microsoft.CodeAnalysis;
using MonoDevelop.Ide.CodeTemplates;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeSnippetService))]
    class CodeSnippetService : ICodeSnippetService
    {
        readonly Lazy<ICodeSnippetFactory> codeSnippetFactory;
        public ICodeSnippetFactory CodeSnippetFactory => codeSnippetFactory.Value;

        [ImportingConstructor]
        public CodeSnippetService(Lazy<ICodeSnippetFactory> codeSnippetFactory)
        {
            this.codeSnippetFactory = codeSnippetFactory;
        }

        public IEnumerable<ICodeSnippet> Snippets => GetIdeCodeSnippets();

        public ICodeSnippet GetCodeSnippetById(string id)
        {
            var templates = CodeTemplateService.GetCodeTemplates("text/x-csharp");

            if (!templates.Any())
            {
                return null;
            }

            var template = templates.FirstOrDefault(t => t.Shortcut == id);

            if (template == null)
            {
                return null;
            }

            return ToCodeSnippet(template);
        }

        internal ICodeSnippet ToCodeSnippet(CodeTemplate template)
        {
            var arguments = template.Variables.Select(v => CodeSnippetFactory.CreateArgument(v.Name, v.Default, v.ToolTip)).ToList();

            return CodeSnippetFactory.CreateSnippet(template.Shortcut, template.Description, template.Code, arguments);
        }

        internal IEnumerable<ICodeSnippet> GetIdeCodeSnippets()
        {
            var templates = CodeTemplateService.GetCodeTemplates("text/x-csharp");

            if (!templates.Any())
            {
                return Enumerable.Empty<ICodeSnippet>();
            }

            return templates.Select(ToCodeSnippet);
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
 