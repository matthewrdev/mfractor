using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using MFractor.Views.ViewModels;
using MFractor.VS.Windows.UI.ViewModels;
using MFractor.Workspace;
using CompilationProject = Microsoft.CodeAnalysis.Project;

namespace MFractor.VS.Windows.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for IconImporterDialog.xaml
    /// </summary>
    public partial class IconImporterDialog : BaseDialogWindow
    {
        public IconImporterDialog(IReadOnlyList<CompilationProject> projects)
        {
            InitializeComponent();

            var viewModel = new IconImporterViewModel(projects);
            viewModel.Completed += OnCompleted;
            DataContext = viewModel;
        }

        void OnCompleted(object sender, EventArgs e) => Close();
    }
}
