using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.BindableProperties
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IBindablePropertyGenerator))]
    class BindablePropertyGenerator : CodeGenerator, IBindablePropertyGenerator
    {
        const string bindableObjectArgumentName = "bindable_object";
        const string bindablePropertyArgumentName = "bindable_property";
        const string controlTypeArgumentName = "control_type";

        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.csharp.bindable_property";

        public override string Documentation => "Generates a bindable property implementation and a proxy property that calls the `BindableProperty` implementation.";

        public override string Name => "Generate Bindable Property";

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new bindable property.", order:0)]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The type of the new bindable property.", order: 1)]
        [CodeSnippetArgument(controlTypeArgumentName, "The control type that the new bindable property has been created inside.", order: int.MaxValue)]
        [CodeSnippetArgument(bindablePropertyArgumentName, "The platforms BindableProperty type.", order: int.MaxValue)]
        [CodeSnippetArgument(bindableObjectArgumentName, "The platforms BindableObject type.", order: int.MaxValue)]
        [CodeSnippetResource("Resources/Snippets/BindableProperty_PropertyChanged.txt")]
        [ExportProperty("When creating the new bindable property, what is the default code snippet MFractor should use? This snippet includes a template that initialises a property changed method.")]
        public ICodeSnippet SnippetWithPropertyChanged { get; set; }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new bindable property.", 0)]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The type of the new bindable property.", 1)]
        [CodeSnippetArgument(controlTypeArgumentName, "The control type that the new bindable property has been created inside.", order: int.MaxValue)]
        [CodeSnippetArgument(bindablePropertyArgumentName, "The platforms BindableProperty type.", order: int.MaxValue)]
        [CodeSnippetResource("Resources/Snippets/BindableProperty.txt")]
        [ExportProperty("When creating the new bindable property, what is the default code snippet MFractor should use?")]
        public ICodeSnippet Snippet { get; set; }

        [Import]
        public IExceptionHandlerGenerator ExceptionHandlerGenerator { get; set; }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(IXamlPlatform platform,
                                                                   string propertyName, 
                                                                   string propertyType,
                                                                   string parentType,
                                                                   BindablePropertyKind bindablePropertyKind)
        {
            var snippet = GetSnippet(bindablePropertyKind);

            return snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, propertyName)
                          .SetArgumentValue(ReservedCodeSnippetArgumentName.Type, propertyType)
                          .SetArgumentValue(controlTypeArgumentName, parentType)
                          .SetArgumentValue(bindablePropertyArgumentName, platform.BindableProperty.MetaType)
                          .SetArgumentValue(bindableObjectArgumentName, platform.BindableObject.MetaType)
                          .AsMembersList();
        }

        public ICodeSnippet GetSnippet(BindablePropertyKind bindablePropertyKind)
        {
            return bindablePropertyKind == BindablePropertyKind.Default ? Snippet : SnippetWithPropertyChanged;
        }
    }
}
