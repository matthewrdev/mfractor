using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.BindableProperties
{
    public enum BindablePropertyKind
    {
        /// <summary>
        /// The bindable property declaration should specify only the property, that is, it should not include a property changed or co-erce value method.
        /// </summary>
        Default,

        /// <summary>
        /// The bindable property declaration shoudl also include a static property changed event.
        /// </summary>
        IncludePropertyChanged,
    }


    /// <summary>
    /// Creates a new bindable properties for a custom control that developers can then consume in XAML with data-binding.
    /// </summary>
    public interface IBindablePropertyGenerator : ICodeGenerator
    {
        /// <summary>
        /// The code snippet to use for the bindable property, including a code template that initialises a static property changed event.
        /// </summary>
        /// <value>The snippet.</value>
        ICodeSnippet SnippetWithPropertyChanged { get; set; }

        /// <summary>
        /// The code snippet to use for the bindable property.
        /// </summary>
        ICodeSnippet Snippet { get; set; }

        /// <summary>
        /// Generate the <see cref="MemberDeclarationSyntax"/> elements that are used to create the bindable property.
        /// </summary>
        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(IXamlPlatform platform,
                                                            string propertyName, 
                                                            string propertyType, 
                                                            string parentType,
                                                            BindablePropertyKind bindablePropertyKind);

        /// <summary>
        /// Gets the code snippet for the <paramref name="bindablePropertyKind"/>.
        /// </summary>
        /// <param name="bindablePropertyKind"></param>
        /// <returns></returns>
        ICodeSnippet GetSnippet(BindablePropertyKind bindablePropertyKind);
    }
}
