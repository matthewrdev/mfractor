
using MFractor.Views.ImageManager;
using MFractor.Images.ImageManager;
using Xwt;

namespace MFractor.VS.Windows.ToolWindows
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.CodeAnalysis;
    using System.Windows;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("2396a0e6-79b1-4f05-8d86-f09b64019f48")]
    public class ImageAssetsToolWindow : ToolWindowPane
    {
        ImageManagerControl imageManagerControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageAssetsToolWindow"/> class.
        /// </summary>
        public ImageAssetsToolWindow() : base(null)
        {
            this.Caption = "Mobile Image Assets";

            imageManagerControl = new ImageManagerControl(default, ImageManagerOptions.Default);

            Content = Toolkit.CurrentEngine.GetNativeWidget(imageManagerControl) as FrameworkElement;
        }

        public void SetOptions(IImageManagerOptions options)
        {
            imageManagerControl.SetOptions(options);
        }

        public void SetSolution(Solution solution)
        {
            imageManagerControl.SetSolution(solution, true);
        }
    }
}
