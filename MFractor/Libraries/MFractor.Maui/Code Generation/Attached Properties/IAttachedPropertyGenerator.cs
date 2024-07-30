using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Maui.CodeGeneration.AttachedProperties
{
    /// <summary>
    /// Creates a new attached properties for a custom control that developers can then consume in XAML with data-binding.
    /// </summary>
    public interface IAttachedPropertyGenerator : ICodeGenerator
    {
        /// <summary>
        /// The code snippet to use for the bindable property.
        /// </summary>
        /// <value>The snippet.</value>
        ICodeSnippet Snippet { get; set; }

        /// <summary>
        /// Generate the <see cref="MemberDeclarationSyntax"/> elements that are used to create an attached property.
        /// </summary>
        /// <returns>The syntax.</returns>
        /// <param name="propertyName">Property name.</param>
        /// <param name="propertyType">Property type.</param>
        /// <param name="parentType">Parent type.</param>
        /// <param name="workspace">Workspace.</param>
        /// <param name="formattingOptions">Formatting options.</param>
        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string propertyName, 
                                                     string propertyType, 
                                                     string parentType, 
                                                     CompilationWorkspace workspace, 
                                                     OptionSet formattingOptions);
    }
}
