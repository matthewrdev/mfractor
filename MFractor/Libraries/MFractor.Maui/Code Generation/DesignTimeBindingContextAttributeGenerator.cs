using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDesignTimeBindingContextAttributeGenerator))]
    class DesignTimeBindingContextAttributeGenerator : CodeGenerator, IDesignTimeBindingContextAttributeGenerator
    {
        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.design_time_binding_context_attribute";

        public override string Name => "Design Time Binding Context Attribute";

        public override string Documentation => "Creates an implementation of the DesignTimeBindingContextAttribute class that can be used to target MFractor towards a binding context for a class where one would normally be unresolved.";

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the design time binding context resides inside.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The type to use as the design time binding context.")]
        [CodeSnippetResource("Resources/Snippets/DesignTimeBindingContextAnnotation.txt")]
        [ExportProperty("The annotation to insert onto the code behind class to target a design time binding context.")]
        public ICodeSnippet AnnotationSnippet
        {
            get; set;
        }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the design time binding context resides inside.")]
        [CodeSnippetResource("Resources/Snippets/DesignTimeBindingContextAttribute.txt")]
        [ExportProperty("The declaration of the design time binding context attribute.")]
        public ICodeSnippet DesignTimeBindingContextAttributeSnippet
        {
            get;
            set;
        }

        [ExportProperty("What is the filename to use when creating the design time binding context?")]
        public string DesignTimeBindingContextAttributeFileName
        {
            get; set;
        } = "DesignTimeBindingContextAttribute.cs";

        [ExportProperty("What is the project folder path to place the  to use when creating the default design time binding context?")]
        public string DesignTimeBindingContextAttributeFolderPath
        {
            get; set;
        } = "Attributes";

        public string DefaultFilePath => Path.Combine(DesignTimeBindingContextAttributeFolderPath, DesignTimeBindingContextAttributeFileName);

        public IReadOnlyList<IWorkUnit> Generate(Project project)
        {
            var @namespace = GetNamespaceFor(project, DesignTimeBindingContextAttributeFolderPath);

            return Generate(project, @namespace, DefaultFilePath);
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project, string namespaceName, string filePath)
        {
            var code = GenerateCode(namespaceName);

            if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".cs";
            }

            return new CreateProjectFileWorkUnit()
            {
                TargetProject = project,
                FileContent = code,
                FilePath = filePath
            }.AsList();
        }

        public string GenerateCode(Project project)
        {
            var @namespace = GetNamespaceFor(project, DesignTimeBindingContextAttributeFolderPath);

            return GenerateCode(@namespace);
        }

        public string GenerateCode(string namespaceName)
        {
            return GenerateSyntax(namespaceName).ToString();
        }

        public CompilationUnitSyntax GenerateSyntax(Project project)
        {
            var @namespace = GetNamespaceFor(project, DesignTimeBindingContextAttributeFolderPath);

            return GenerateSyntax(@namespace);
        }

        public CompilationUnitSyntax GenerateSyntax(string namespaceName)
        {
            DesignTimeBindingContextAttributeSnippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, namespaceName);

            return DesignTimeBindingContextAttributeSnippet.AsCompilationUnit();
        }

        public string GetNamespaceFor(Project project, string namespacePath)
        {
            var defaultNamespace = ProjectService.GetDefaultNamespace(project);

            if (string.IsNullOrEmpty(namespacePath))
            {
                return defaultNamespace;
            }

            if (!namespacePath.StartsWith(".", StringComparison.Ordinal))
            {
                namespacePath = "." + namespacePath;
            }

            return defaultNamespace + namespacePath;
        }
    }
}