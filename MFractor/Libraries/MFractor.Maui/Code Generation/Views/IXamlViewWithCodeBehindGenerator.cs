using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Work.WorkUnits;
using MFractor.Code;
using MFractor.Maui.Analysis;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using MFractor.Maui.Syntax;
using MFractor.Maui.Xmlns;
using MFractor.Work;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.CodeGeneration.Views
{
    /// <summary>
    /// The XAML view with code behind generator 
    /// </summary>
    public interface IXamlViewWithCodeBehindGenerator : ICodeGenerator
    {
        /// <summary>
        /// What is the default implementation of the new XAML control's XAML file?
        /// </summary>
        /// <value>The view snippet.</value>
        ICodeSnippet ViewSnippet { get; set; }

        /// <summary>
        /// What is the default implementation of the new XAML control's code behind class?
        /// </summary>
        /// <value>The code behind snippet.</value>
        ICodeSnippet CodeBehindSnippet { get; set; }

        /// <summary>
        /// Generates the workUnits.
        /// </summary>
        /// <returns>The workUnits.</returns>
        /// <param name="className">Class name.</param>
        /// <param name="namespaceName">Namespace name.</param>
        /// <param name="project">Project.</param>
        /// <param name="rootNode">Root node.</param>
        /// <param name="namespaces">Namespaces.</param>
        /// <param name="codeBehindType">Code behind type.</param>
        IReadOnlyList<IWorkUnit> Generate(string className,
                                        string namespaceName,
                                        Project project,
                                        string folder, 
                                        IXmlFormattingPolicy xmlPolicy,
                                        XmlNode rootNode,
                                        IXamlPlatform platform,
                                        IXamlNamespaceCollection namespaces,
                                        INamedTypeSymbol codeBehindType);

        /// <summary>
        /// Generate the workUnits to create a new XAML view and code behind.
        /// </summary>
        /// <returns>The generate.</returns>
        /// <param name="className">Class name.</param>
        /// <param name="namespaceName">Namespace name.</param>
        /// <param name="projectIdentifier">Project identifier.</param>
        /// <param name="folder">Folder.</param>
        /// <param name="xmlPolicy">Xml policy.</param>
        /// <param name="rootNode">Root node.</param>
        /// <param name="namespaces">Namespaces.</param>
        /// <param name="codeBehindType">Code behind type.</param>
        IReadOnlyList<IWorkUnit> Generate(string className,
                                        string namespaceName,
                                        ProjectIdentifier projectIdentifier,
                                        CompilationWorkspace workspace,
                                        string folder,
                                        IXmlFormattingPolicy xmlPolicy,
                                        XmlNode rootNode,
                                        IXamlPlatform platform,
                                        IXamlNamespaceCollection namespaces,
                                        INamedTypeSymbol codeBehindType);

        /// <summary>
        /// Generates the workUnits to create a new XAML view and code behind
        /// </summary>
        /// <returns>The workUnits.</returns>
        /// <param name="className">Class name.</param>
        /// <param name="namespaceName">Namespace name.</param>
        /// <param name="project">Project.</param>
        IReadOnlyList<IWorkUnit> Generate(string className,
                                        string namespaceName,
                                        string namespaceXmlnsName,
                                        Project project,
                                        IXamlPlatform platform,
                                        string folder,
                                        string baseClass);

        /// <summary>
        /// Generate the specified className, namespaceName, projectIdentifier, folder and baseClass.
        /// </summary>
        /// <returns>The generate.</returns>
        /// <param name="className">Class name.</param>
        /// <param name="namespaceName">Namespace name.</param>
        /// <param name="projectIdentifier">Project identifier.</param>
        /// <param name="folder">Folder.</param>
        /// <param name="baseClass">Base class.</param>
        IReadOnlyList<IWorkUnit> Generate(string className,
                                        string namespaceName,
                                        string namespaceXmlnsName,
                                        ProjectIdentifier projectIdentifier,
                                        CompilationWorkspace workspace,
                                        IXamlPlatform platform,
                                        string folder,
                                        string baseClass);

        /// <summary>
        /// Generates the xaml view using the provided <paramref name="className"/>, <paramref name="namespaceName"/> and <paramref name="baseClass"/>.
        /// </summary>
        /// <returns>The xaml.</returns>
        /// <param name="className">Class name.</param>
        /// <param name="namespaceName">Namespace name.</param>
        /// <param name="baseClass">The base class</param>
		string GenerateXAMLView(string className,
                                string namespaceName,
                                string baseClass,
                                string xmlnsPrefix,
                                IXamlPlatform platform,
                                Compilation compilation);


        /// <summary>
        /// Generates the XAML View from the node.
        /// </summary>
        /// <returns>The XAMLV iew from node.</returns>
        /// <param name="className">Class name.</param>
        /// <param name="namespaceName">Namespace name.</param>
        /// <param name="rootNode">Root node.</param>
        /// <param name="namespaces">Namespaces.</param>
        XmlNode GenerateXAMLView(string className,
                                 string namespaceName,
                                 XmlNode rootNode,
                                 IXamlPlatform platform,
                                 IXamlNamespaceCollection namespaces);

        /// <summary>
        /// Generates the code behind.
        /// </summary>
        /// <returns>The code behind.</returns>
        /// <param name="className">Class name.</param>
        /// <param name="baseType">Base type.</param>
        /// <param name="namespaceName">Namespace name.</param>
        /// <param name="workspace">Workspace.</param>
        /// <param name="formattingOptions">Formatting options.</param>
        CompilationUnitSyntax GenerateCodeBehind(string className, 
                                                 string baseType, 
                                                 string namespaceName, 
                                                 CompilationWorkspace workspace, 
                                                 OptionSet formattingOptions);
    }
}
