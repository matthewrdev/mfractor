using System;
using System.Collections.Generic;
using MFractor.Code.Documents;
using MFractor.Maui.Xmlns;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui
{
    /// <summary>
    /// A parsed XAML document.
    /// </summary>
	public class ParsedXamlDocument : ParsedXmlDocument, IParsedXamlDocument
    {
        /// <summary>
        /// What is the default BindingContext for this XAML file?
        /// <para/>
        /// This may be null if no default binding context could be resolved.
        /// </summary>
        /// <value>The binding context.</value>
        public INamedTypeSymbol BindingContext { get; }

        /// <summary>
        /// The type symbol of the code behind file for this XAML file.
        /// </summary>
        /// <value>The code behind symbol.</value>
        public INamedTypeSymbol CodeBehindSymbol { get; }

        /// <summary>
        /// The class syntax of the code behind file for this XAML file.
        /// </summary>
        /// <value>The code behind syntax.</value>
        public ClassDeclarationSyntax CodeBehindSyntax { get; }

        /// <summary>
        /// The XAML namespaces defined by this XAML file.
        /// </summary>
        /// <value>The namespaces.</value>
        public IXamlNamespaceCollection Namespaces { get; }

        /// <summary>
        /// The XmlnsDefitions available in the current app domain.
        /// </summary>
        /// <value>The namespaces.</value>
        public IXmlnsDefinitionCollection XmlnsDefinitions { get; }

        public XmlSyntaxTree XamlSyntaxTree => GetSyntaxTree();

        public XmlNode XamlSyntaxRoot => XamlSyntaxTree?.Root;

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Maui.ParsedXamlDocument"/> class.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="syntaxTree">Syntax tree.</param>
        /// <param name="bindingContext">Binding context.</param>
        /// <param name="codeBehindSymbol">Code behind symbol.</param>
        /// <param name="codeBehindSyntax">Code behind syntax.</param>
        /// <param name="projectFile">Project file.</param>
        public ParsedXamlDocument(string filePath,
                                  XmlSyntaxTree syntaxTree,
                                  INamedTypeSymbol bindingContext,
                                  INamedTypeSymbol codeBehindSymbol,
                                  ClassDeclarationSyntax codeBehindSyntax,
                                  IProjectFile projectFile,
                                  IXamlNamespaceCollection namespaces,
                                  IXmlnsDefinitionCollection xmlnsDefinitions)
            : base(filePath, syntaxTree, projectFile)
        {
            BindingContext = bindingContext;
            CodeBehindSymbol = codeBehindSymbol;
            CodeBehindSyntax = codeBehindSyntax;
            Namespaces = namespaces ?? throw new ArgumentNullException(nameof(namespaces));
            XmlnsDefinitions = xmlnsDefinitions ?? throw new ArgumentNullException(nameof(xmlnsDefinitions));
        }
    }
}
