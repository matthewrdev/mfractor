using System;
using System.ComponentModel.Composition;
using MFractor.CodeSnippets;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;

using MFractor.Maui.CustomRenderers;
using MFractor.Workspace;

namespace MFractor.Maui.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAndroidCustomRendererConfiguration))]
    class AndroidCustomRendererConfiguration : Configurable, IAndroidCustomRendererConfiguration
    {
        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public AndroidCustomRendererConfiguration(Lazy<IProjectService> projectService)
        {
            this.projectService = projectService;
        }

        [ExportProperty("The code snippet to use for customised pages.")]
        [CodeSnippetResource("Resources/Snippets/Android/CustomRenderer.txt")]
        [CodeSnippetArgument("control_type", "The fully qualified type of the page that this custom renderer is for.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.ControlName, "The name of the page that this custom renderer is for.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the custom renderer will be placed into.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererName, "The name of the new custom renderer.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererControl, "The name of the base control type. For example, if your control is `MyCustomLabel` that extends a `Label`, **renderer_control** will be `Label`.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.BaseType, "The fully qualified type of the renderers base type.")]
        public ICodeSnippet PageRendererSnippet { get; set; }

        [ExportProperty("The code snippet to use for customised layouts.")]
        [CodeSnippetResource("Resources/Snippets/Android/CustomRenderer.txt")]
        [CodeSnippetArgument("control_type", "The fully qualified type of the layout that this custom renderer is for.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.ControlName, "The name of the layout that this custom renderer is for.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the custom renderer will be placed into.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererName, "The name of the new custom renderer.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererControl, "The name of the base control type. For example, if your control is `MyCustomLabel` that extends a `Label`, **renderer_control** will be `Label`.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.BaseType, "The fully qualified type of the renderers base type.")]
        public ICodeSnippet LayoutRendererSnippet { get; set; }

        [ExportProperty("The code snippet to use for customised controls.")]
        [CodeSnippetResource("Resources/Snippets/Android/ControlRenderer.txt")]
        [CodeSnippetArgument("control_type", "The fully qualified type of the control that this custom renderer is for.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.ControlName, "The name of the control that this custom renderer is for.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the custom renderer will be placed into.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererName, "The name of the new custom renderer.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererControl, "The name of the base control type. For example, if your control is `MyCustomLabel` that extends a `Label`, **renderer_control** will be `Label`.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.BaseType, "The fully qualified type of the renderers base type.")]
        public ICodeSnippet ViewRendererSnippet { get; set; }

        [ExportProperty("The code snippet to use for customised cells in ListViews.")]
        [CodeSnippetResource("Resources/Snippets/Android/CellRenderer.txt")]
        [CodeSnippetArgument("control_type", "The fully qualified type of the control that this custom renderer is for.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.ControlName, "The name of the control that this custom renderer is for.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the custom renderer will be placed into.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererName, "The name of the new custom renderer.")]
        [CodeSnippetArgument(RendererCodeSnippetArguments.RendererControl, "The name of the base control type. For example, if your control is `MyCustomLabel` that extends a `Label`, **renderer_control** will be `Label`.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.BaseType, "The fully qualified type of the renderers base type.")]
        public ICodeSnippet CellRendererSnippet { get; set; }

        public override string Identifier => "com.mfractor.configuration.xaml.android_custom_renderers";

        public override string Name => "Android Custom Renderer Code Snippets";

        public override string Documentation => "Groups the Android specific custom renderer code snippets for pages, layouts, controls and view cells into a single configuration point. When customising the code snippets for Android, this configuration should be changed in the Android project rather than the PCL/Shared Project/netstandard library.";
    }
}