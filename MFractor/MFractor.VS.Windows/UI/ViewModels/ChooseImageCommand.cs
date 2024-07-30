using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MFractor.Images;
using Microsoft.Win32;

namespace MFractor.VS.Windows.UI.ViewModels
{
    public class ChooseImageEventArgs : EventArgs
    {
        public string ImageFilePath { get; }

        public ChooseImageEventArgs(string imageFilePath)
        {
            ImageFilePath = imageFilePath;
        }
    }

    public interface IChooseImageCommand : ICommand
    {
        event EventHandler<ChooseImageEventArgs> ImageChose;
    }

    [Export(typeof(IChooseImageCommand))]
    class ChooseImageCommand : IChooseImageCommand
    {
        public event EventHandler<ChooseImageEventArgs> ImageChose;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose an image to import";
            openFileDialog.Filter = "Image files (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() ?? false)
            {
                var eventArgs = new ChooseImageEventArgs(openFileDialog.FileName);
                ImageChose?.Invoke(this, eventArgs);
            }
        }
    }
}
