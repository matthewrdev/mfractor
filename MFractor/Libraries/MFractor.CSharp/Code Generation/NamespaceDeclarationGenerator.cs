using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(INamespaceDeclarationGenerator))]
    class NamespaceDeclarationGenerator : CSharpCodeGenerator, INamespaceDeclarationGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.namespace_declaration";

        public override string Name => "Namespace Declaration";

        public override string Documentation => "Generates a new namespace declaration syntax element";

        public string CleanNamespaceName(string @namespace)
        {
            if (string.IsNullOrEmpty(@namespace))
            {
                return string.Empty;
            }

            @namespace = @namespace.Replace("/", ".")
                                   .Replace("\\", ".")
                                   .Replace(" ", "");

            var components = @namespace.Split('.')
                                       .Select(CSharpNameHelper.ConvertToValidCSharpName);

            @namespace = string.Join(".", components);

            return @namespace;
        }

        public NamespaceDeclarationSyntax GenerateSyntax(string name)
        {
            name = CleanNamespaceName(name);

            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(name))
                    .WithNamespaceKeyword(
                        SyntaxFactory.Token(
                            SyntaxFactory.TriviaList(),
                            SyntaxKind.NamespaceKeyword,
                            SyntaxFactory.TriviaList()));
        }

        public string GetNamespaceFor(ProjectIdentifier projectIdentifier, IEnumerable<string> folderPath)
        {
            folderPath = folderPath ?? Enumerable.Empty<string>();

            var path = string.Join(".", folderPath);

            return GetNamespaceFor(projectIdentifier, path);
        }

        public string GetNamespaceFor(ProjectIdentifier projectIdentifier, string folderPath)
        {
            var namespaceName = ProjectService.GetDefaultNamespace(projectIdentifier);

            if (string.IsNullOrEmpty(folderPath) == false)
            {
                namespaceName += "." + CleanNamespaceName(folderPath);
            }

            return namespaceName;
        }

        public string GetNamespaceFor(ProjectIdentifier projectIdentifier, IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return ProjectService.GetDefaultNamespace(projectIdentifier);
            }

            return GetNamespaceFor(projectIdentifier, projectFile.ProjectFolders);
        }

        public string GetNamespaceFor(Project project, IEnumerable<string> folderPath)
        {
            return GetNamespaceFor(project.GetIdentifier(), folderPath);
        }

        public string GetNamespaceFor(Project project, string folderPath)
        {
            return GetNamespaceFor(project.GetIdentifier(), folderPath);
        }

        public string GetNamespaceFor(Project project, IProjectFile projectFile)
        {
            return GetNamespaceFor(project.GetIdentifier(), projectFile);
        }
    }
}
