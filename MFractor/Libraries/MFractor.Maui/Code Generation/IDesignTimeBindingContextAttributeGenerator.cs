using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration
{
    /// <summary>
    /// Generates an implementation of the DesignTimeBindingContextAttribute.
    /// </summary>
    public interface IDesignTimeBindingContextAttributeGenerator : ICodeGenerator
    {
        /// <summary>
        /// When adding a DesignTimeBindingContextAttribute annotation to a class, what is it?
        /// </summary>
        /// <value>The annotation snippet.</value>
        ICodeSnippet AnnotationSnippet { get; set; }

        /// <summary>
        /// What is the default file name to use for the DesignTimeBindingContextAttribute
        /// </summary>
        /// <value>The name of the design time binding context attribute file.</value>
        string DesignTimeBindingContextAttributeFileName { get; set; }

        /// <summary>
        /// What is the default folder path that the DesignTimeBindingContextAttribute be placed into?
        /// </summary>
        /// <value>The design time binding context attribute folder path.</value>
        string DesignTimeBindingContextAttributeFolderPath { get; set; }

        /// <summary>
        /// The code snippet for generating 
        /// </summary>
        /// <value>The design time binding context attribute snippet.</value>
        ICodeSnippet DesignTimeBindingContextAttributeSnippet { get; set; }

        string GenerateCode(Project project);

        string GenerateCode(string namespaceName);

        CompilationUnitSyntax GenerateSyntax(Project project);

        CompilationUnitSyntax GenerateSyntax(string namespaceName);

        IReadOnlyList<IWorkUnit> Generate(Project project);

        IReadOnlyList<IWorkUnit> Generate(Project project, string namespaceName, string filePath);

        string GetNamespaceFor(Project project, string namespacePath);
    }
}
