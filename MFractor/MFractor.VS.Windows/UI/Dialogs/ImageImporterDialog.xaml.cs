using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MFractor.IOC;
using MFractor.Utilities;
using MFractor.VS.Windows.UI.ViewModels;
using MFractor.Workspace;
using Microsoft.VisualStudio.PlatformUI;

namespace MFractor.VS.Windows.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for ImageImporter.xaml
    /// </summary>
    public partial class ImageImporterDialog : BaseDialogWindow
    {
        const string defaultImagePreviewSource = "/MFractor.VS.Windows;component/Assets/mfractor_logo_grayscale.png";

        public ImageImporterDialog()
        {
            InitializeComponent();

            var workspaceService = Resolver.Resolve<IWorkspaceService>();
            var solution = workspaceService.CurrentWorkspace.CurrentSolution;
            var projects = solution.GetMobileProjects();

            var imageImporterViewModel = new ImageImporterViewModel(projects, defaultImagePreviewSource);
            imageImporterViewModel.Completed += ImageImporterViewModel_Completed;

            DataContext = imageImporterViewModel;
        }

        void ImageImporterViewModel_Completed(object sender, EventArgs e) => this.Close();

        void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsSignedInteger(e.Text);
        }

        static bool IsSignedInteger(string text) => int.TryParse(text, out var value) && value >= 0;
    }
}
