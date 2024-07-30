using MFractor.Code.Documents;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui
{
    /// <summary>
    /// A parsed XAML document.
    /// </summary>
    public interface IParsedXamlDocument : IParsedXmlDocument
    {
        /// <summary>
        /// What is the default BindingContext for this XAML file?
        /// <para/>
        /// This may be null if no default binding context could be resolved.
        /// </summary>
        INamedTypeSymbol BindingContext { get; }

        /// <summary>
        /// The type symbol of the code behind file for this XAML file.
        /// </summary>
        INamedTypeSymbol CodeBehindSymbol { get; }

        /// <summary>
        /// The class syntax of the code behind file for this XAML file.
        /// </summary>
        ClassDeclarationSyntax CodeBehindSyntax { get; }

        IXamlNamespaceCollection Namespaces { get; }

        /// <summary>
        /// The XmlnsDefitions available in the current app domain.
        /// </summary>
        IXmlnsDefinitionCollection XmlnsDefinitions { get; }

        XmlSyntaxTree XamlSyntaxTree { get; }

        XmlNode XamlSyntaxRoot { get; }
    }
}