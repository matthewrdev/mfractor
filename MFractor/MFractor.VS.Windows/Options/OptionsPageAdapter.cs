using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MFractor.Views.Settings;
using Microsoft.VisualStudio.Shell;
using Xwt;

namespace MFractor.VS.Windows.UI.OptionsPage
{
    public class OptionsPageAdapter<TOptions> : UIElementDialogPage, INotifyPropertyChanged where TOptions : IOptionsWidget, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;

        TOptions Options { get; } = new TOptions();
        
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        UIElement control;

        protected override UIElement Child
        {
            get
            {
                if (control == null)
                {
                    var widget = Options.Widget;

                    var element = Toolkit.CurrentEngine.GetNativeWidget(widget) as FrameworkElement;

                    control = element;
                }

                return control;
            }
        }

        public override void SaveSettingsToStorage()
        {
            Options.ApplyChanges();
        }
    }
}
