using System.ComponentModel.Composition;
using System.Windows;
using MFractor.Views;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IRootWindowService))]
    class RootWindowService : IRootWindowService
    {
        public Xwt.WindowFrame RootWindowFrame => Xwt.Toolkit.CurrentEngine.WrapWindow(Application.Current.MainWindow);
    }
}
