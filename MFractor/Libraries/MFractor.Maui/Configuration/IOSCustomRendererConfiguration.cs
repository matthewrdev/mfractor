using System.ComponentModel.Composition;
using MFractor.CodeSnippets;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;

using MFractor.Maui.CustomRenderers;

namespace MFractor.Maui.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IIOSCustomRendererConfiguration))]
    class IOSCustomRendererConfiguration : Configurable, IIOSCustomRendererConfiguration
    {
        [ExportProperty("The code snippet to use for customised pages.")]
        [CodeSnippetResource("Resources/Snippets/iOS/CustomRenderer.txt")]
        [CodeSnippetArgument("control_type", "The fully qualified type of the page that this custom renderer is for.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.ControlName, "The name of the page that this custom renderer is for.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the custom renderer will be placed into.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererName, "The name of the new custom renderer.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererControl, "The name of the base control type. For example, if your control is `MyCustomLabel` that extends a `Label`, **renderer_control** will be `Label`.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.BaseType, "The fully qualified type of the renderers base type.")]
        public ICodeSnippet PageRendererSnippet { get; set; }

        [ExportProperty("The code snippet to use for customised layouts.")]
        [CodeSnippetResource("Resources/Snippets/iOS/CustomRenderer.txt")]
        [CodeSnippetArgument("control_type", "The fully qualified type of the layout that this custom renderer is for.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.ControlName, "The name of the layou that this custom renderer is for.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the custom renderer will be placed into.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererName, "The name of the new custom renderer.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererControl, "The name of the base control type. For example, if your control is `MyCustomLabel` that extends a `Label`, **renderer_control** will be `Label`.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.BaseType, "The fully qualified type of the renderers base type.")]
        public ICodeSnippet LayoutRendererSnippet { get; set; }

        [ExportProperty("The code snippet to use for cu stomised controls.")]
        [CodeSnippetResource("Resources/Snippets/iOS/ControlRenderer.txt")]
        [CodeSnippetArgument("control_type", "The fully qualified type of the control that this custom renderer is for.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.ControlName, "The name of the control that this custom renderer is for.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the custom renderer will be placed into.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererName, "The name of the new custom renderer.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererControl, "The name of the base control type. For example, if your control is `MyCustomLabel` that extends a `Label`, **renderer_control** will be `Label`.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.BaseType, "The fully qualified type of the renderers base type.")]
        public ICodeSnippet ViewRendererSnippet { get; set; }

        [ExportProperty("The code snippet to use for customised cells in ListViews.")]
        [CodeSnippetResource("Resources/Snippets/iOS/CellRenderer.txt")]
        [CodeSnippetArgument("control_type", "The fully qualified type of the control that this custom renderer is for.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.ControlName, "The name of the control that this custom renderer is for.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the custom renderer will be placed into.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererName, "The name of the new custom renderer.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererControl, "The name of the base control type. For example, if your control is `MyCustomLabel` that extends a `Label`, **renderer_control** will be `Label`.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.BaseType, "The fully qualified type of the renderers base type.")]
        public ICodeSnippet CellRendererSnippet { get; set; }

        public override string Identifier => "com.mfractor.configuration.xaml.ios_custom_renderers";

        public override string Name => "IOS Custom Renderer Code Snippets";

        public override string Documentation => "Groups the iOS specific custom renderer code snippets for pages, layouts, controls and view cells into a single configuration point. When customising the code snippets for iOS, this configuration should be changed in the iOS project rather than the PCL/Shared Project/netstandard library.";
    }
}