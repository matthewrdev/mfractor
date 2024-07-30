using System;

namespace MFractor.Maui.CustomRenderers
{
    /// <summary>
    /// A collection of argument constants for MFractors custom renderer code snippets.
    /// </summary>
    public class RendererCodeSnippetArguments
    {
        /// <summary>
        /// The name of the layout that this custom renderer is for.
        /// </summary>
        public const string ControlName = "control_name";

        /// <summary>
        /// The name of the new custom renderer.
        /// </summary>
        public const string RendererName = "renderer_name";

        /// <summary>
        /// The name of the base control type. For example, if your control is MyCustomLabel that extends a Label, 'renderer_control' will be Label.
        /// </summary>
        public const string RendererControl = "renderer_control";
    }
}
