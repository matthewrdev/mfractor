using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using MFractor.Images.Importing;
using MFractor.Images.Models;
using MFractor.IOC;
using MFractor.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Views.ViewModels
{
    public class IconImporterViewModel : ObservableBase
    {
        IImagePicker imagePicker;
        IDialogsService dialogService;
        IIconImporterService iconImporterService;

        public event EventHandler Completed;

        string imageFileName;
        public string ImageFileName
        {
            get => imageFileName;
            set
            {
                SetProperty(ref imageFileName, value);
                OnPropertyChanged(nameof(IsValidFileName));
            }
        }

        public bool IsValidFileName => !string.IsNullOrWhiteSpace(ImageFileName) && File.Exists(ImageFileName);

        public ObservableCollection<TargetProjectViewModel> Projects { get; }

        public IEnumerable<AppIconSet> IconSets { get; }

        public ICommand OpenImageCommand => new AsyncMvvmCommand(async () =>
        {
            var fileName = await imagePicker.PickAsync("Choose an image for the icon.");
            if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
            {
                UpdateImage(fileName);
            }
        });

        public ICommand ImportIconCommand => new AsyncMvvmCommand(async () =>
        {
            if (!Validate())
            {
                return;
            }

            if (await ImportIcons())
            {
                Completed?.Invoke(null, EventArgs.Empty);
            }
            else
            {
                dialogService.ShowError("Couldn't import the images.");
            }
        });

        public IconImporterViewModel(IReadOnlyList<Project> projects)
        {
            Projects = new ObservableCollection<TargetProjectViewModel>(projects.Select(p => new TargetProjectViewModel(p)));
            imagePicker = Resolver.Resolve<IImagePicker>();
            dialogService = Resolver.Resolve<IDialogsService>();
            iconImporterService = Resolver.Resolve<IIconImporterService>();

            // TODO: Refactor to use new models on Windows
            //IconSets = new List<AppIconSet>(AppIconSet.AllSets);
        }

        void UpdateImage(string filePath)
        {
            // TODO: Fix
            //ImageFileName = filePath;
            //foreach (var device in IconSets)
            //{
            //    device.SetImageFileName(ImageFileName);
            //}
        }

        /// <summary>
        /// Process the importing
        /// </summary>
        async Task<bool> ImportIcons()
        {
            // 1. Need to transform each selected project and each of the device icon groups into import operations
            // 2. Process the work units under Image Import Service
            var success = true;

            foreach (var project in Projects.Where(p => p.IsIncluded))
            {
                success &= await iconImporterService.ImportIconAsync(IconSets.Where(s => s.IsSelected), ImageFileName, project.Project);
            }

            return true;
        }

        bool Validate()
        {
            if (!IsValidFileName)
            {
                dialogService.ShowError("You must select a icon image to import.");
                return false;
            }

            if (!IconSets.Any(d => d.IsSelected))
            {
                dialogService.ShowError("You must select at least one device type to export.");
                return false;
            }

            return true;
        }
    }
}
