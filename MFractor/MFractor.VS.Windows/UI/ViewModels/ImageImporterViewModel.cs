using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MFractor.Android.Helpers;
using MFractor.Images;
using MFractor.Images.Importing;
using MFractor.Images.Utilities;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Models;
using MFractor.Progress;
using MFractor.Utilities;
using MFractor.Views.ViewModels;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.VS.Windows.UI.ViewModels
{
    public class ImageImporterViewModel : ObservableBase
    {
        IWorkEngine workEngine;
        ImageDescriptor imageDescriptor;
        IDialogsService dialogsService;
        ILicensingService licensingService;
        IImageImporterService imageImporterService;

        public event EventHandler Completed;

        string imageFileName;
        public string ImageFileName
        {
            get => imageFileName;
            set
            {
                SetProperty(ref imageFileName, value);
                OnPropertyChanged(nameof(HasImage));
                OnPropertyChanged(nameof(IsValidFileName));
            }
        }

        public bool IsValidFileName => !string.IsNullOrWhiteSpace(ImageFileName);

        string resourceName;
        public string ResourceName
        {
            get => resourceName;
            set
            {
                SetProperty(ref resourceName, value);
                OnPropertyChanged(nameof(IsValidResourceName));
                OnPropertyChanged(nameof(IsValidAndroidResourceName));
            }
        }

        public bool IsValidResourceName => !string.IsNullOrWhiteSpace(ResourceName) && AndroidResourceNameHelper.ResourceNameRegex.IsMatch(ResourceName);

        public bool IsValidAndroidResourceName => string.IsNullOrWhiteSpace(ResourceName) || AndroidResourceNameHelper.ResourceNameRegex.IsMatch(ResourceName);

        string imagePreviewSource;
        public string ImagePreviewSource
        {
            get => imagePreviewSource;
            set => SetProperty(ref imagePreviewSource, value);
        }

        public bool HasImage => !string.IsNullOrEmpty(ImageFileName) && File.Exists(ImageFileName);

        int width;
        public int Width
        {
            get => width;
            private set
            {
                SetProperty(ref width, value);
                OnPropertyChanged(nameof(WidthDescription));
                OnPropertyChanged(nameof(SizeDescription));
            }
        }

        int height;
        public int Height
        {
            get => height;
            private set
            {
                SetProperty(ref height, value);
                OnPropertyChanged(nameof(HeightDescription));
                OnPropertyChanged(nameof(SizeDescription));
            }
        }

        public string WidthDescription => Width > 0 ? Width.ToString() : "NA";

        public string HeightDescription => Height > 0 ? Height.ToString() : "NA";

        public string SizeDescription => $"Width: {WidthDescription} | Height: {HeightDescription}";

        public double SizeAspect => (double)Width / ((Height == 0) ? 1.0 : (double)Height);


        bool isResizeImage = true;
        public bool IsResizeImage
        {
            get => isResizeImage;
            set => SetProperty(ref isResizeImage, value);
        }

        int resizeWidth;
        public int ResizeWidth
        {
            get => resizeWidth;
            set => OnResizeWidthChanged(value, true);
        }

        int resizeHeight;
        public int ResizeHeight
        {
            get => resizeHeight;
            set => OnResizeHeightChanged(value, true);
        }

        public bool IsProjectSelected => SelectedProject != null;

        ImageImporterTargetProjectViewModel selectedProject;
        public ImageImporterTargetProjectViewModel SelectedProject
        {
            get => selectedProject;
            set
            {
                SetProperty(ref selectedProject, value);
                OnPropertyChanged(nameof(IsProjectSelected));

                if (imageDescriptor != null)
                {
                    UpdatePreviews();
                }
            }
        }

        public IEnumerable<ImageImporterTargetProjectViewModel> TargetProjects { get; }

        public ObservableCollection<ImageImportPreviewViewModel> Previews { get; } = new ObservableCollection<ImageImportPreviewViewModel>();

        public ICommand ImportCommand => new AsyncMvvmCommand(async () =>
        {
            if (!await ValidateAsync())
            {
                return;
            }

            var workUnits = GetIncludedProjectsImportOperations();

            if (await imageImporterService.Import(workUnits, new StubProgressMonitor()))
            {
                Completed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                dialogsService.ShowError("Couldn't import the images.");
            }
        });

        public ICommand FixResourceNameCommand => new MvvmCommand(() =>
        {
            ResourceName = string.Join("", ResourceName.Select(c => AndroidResourceNameHelper.ResourceNameRegex.IsMatch(c.ToString()) ? c.ToString() : ""));
        });

        public IChooseImageCommand ChooseImageCommand { get; }

        public ImageImporterViewModel(List<Project> projects, string imagePreviewSource)
        {
            workEngine = Resolver.Resolve<IWorkEngine>();
            dialogsService = Resolver.Resolve<IDialogsService>();
            licensingService = Resolver.Resolve<ILicensingService>();
            imageImporterService = Resolver.Resolve<IImageImporterService>();

            ChooseImageCommand = Resolver.Resolve<IChooseImageCommand>();
            ChooseImageCommand.ImageChose += (s, e) => UpdateImage(e.ImageFilePath);

            TargetProjects = projects
                .Where(p => p.IsAndroidProject() || p.IsAppleUnifiedProject())
                .Select(p => new ImageImporterTargetProjectViewModel(p) { IsIncluded = true })
                .ToList();

            // Ensures the Previews Panel is updated when the user selects a new density/image type
            foreach (var project in TargetProjects)
            {
                project.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ImageImporterTargetProjectViewModel.SelectedImageDensity) ||
                        e.PropertyName == nameof(ImageImporterTargetProjectViewModel.SelectedImageType))
                    {
                        UpdatePreviews();
                    }
                };
            }

            ImagePreviewSource = imagePreviewSource;
        }

        void OnResizeWidthChanged(int newWidth, bool propagateToHeight)
        {
            SetProperty(ref resizeWidth, newWidth, nameof(ResizeWidth));
            if (propagateToHeight)
            {
                var newHeigth = GetRoundedAspect(newWidth);
                OnResizeHeightChanged(newHeigth, false);
                UpdatePreviews();
            }
        }

        void OnResizeHeightChanged(int newHeigth, bool propagateToWidth)
        {
            SetProperty(ref resizeHeight, newHeigth, nameof(ResizeHeight));
            if (propagateToWidth)
            {
                var newWidth = GetRoundedAspect(newHeigth);
                OnResizeWidthChanged(newWidth, false);
                UpdatePreviews();
            }
        }

        int GetRoundedAspect(int size) => (int)Math.Round(size * SizeAspect);

        void UpdateImage(string filePath)
        {
            ImageFileName = filePath;
            ImagePreviewSource = filePath;
            ResourceName = new FileInfo(filePath).Name;

            imageDescriptor = new ImageDescriptor(filePath);
            Width = imageDescriptor.Width;
            Height = imageDescriptor.Height;
            ResizeWidth = Width;
            ResizeHeight = Height;
        }

        void UpdatePreviews()
        {
            Previews.Clear();

            if (SelectedProject == null)
            {
                return;
            }

            var operation = GetSelectedProjectImportOperation();
            foreach (var imageDensity in SelectedProject.ConsideredImageDensities)
            {
                Previews.Add(new ImageImportPreviewViewModel(imageDescriptor.ToImportDescriptor(imageDensity, operation)));
            }
        }

        IEnumerable<ImportImageOperation> GetIncludedProjectsImportOperations() =>
            TargetProjects
                .Where(p => p.IsIncluded)
                .Select(p => GetTargetProjectImportOperation(p));

        ImportImageOperation GetSelectedProjectImportOperation() =>
            SelectedProject == null
            ? null
            : GetTargetProjectImportOperation(SelectedProject);

        ImportImageOperation GetTargetProjectImportOperation(ImageImporterTargetProjectViewModel targetProject) =>
            new ImportImageOperation(
                targetProject.Project,
                ResourceName,
                imageDescriptor.FilePath,
                string.Empty,
                targetProject.SelectedImageType,
                GetIncludedProjectsMaximumDensity(),
                targetProject.ConsideredImageDensities.ToList(),
                GetDesiredSize(),
                targetProject.FolderPath);

        ImageSize GetDesiredSize() => IsResizeImage && ResizeWidth >0 && ResizeHeight > 0 ? new ImageSize(ResizeWidth, ResizeHeight) : imageDescriptor.Size;

        ImageDensity GetIncludedProjectsMaximumDensity()
        {
            ImageDensity maxDensity = null;

            foreach (var project in TargetProjects.Where(p => p.IsIncluded))
            {
                var density = project.SelectedImageDensity;
                if (maxDensity == null || maxDensity.Scale < density.Scale)
                {
                    maxDensity = density;
                }
            }

            return maxDensity;
        }

        async Task<bool> ValidateAsync()
        {
            if (IsResizeImage && (ResizeWidth <= 0 || ResizeHeight <= 0))
            {
                dialogsService.ShowError("To resize the image Width and Height must be greater than 0.");
                return false;
            }

            if (!TargetProjects.Any(p => p.IsIncluded))
            {
                dialogsService.ShowError("You must select at least one project to import images.");
                return false;
            }

            if (!ValidateAndroidProjects())
            {
                dialogsService.ShowError($"The name \"{ResourceName}\" violates Androids naming restrictions. Accepted characters are 0-9, a-z, A-Z, . and _.\n\n\"{ResourceName}\" will cause build errors in Android projects.\n\nPlease choose a different name or use the clean image name wand.");
                return false;
            }

            if (!licensingService.IsPaid)
            {
                await ShowLicensePromptAsync();
                return false;
            }

            return true;
        }

        bool ValidateAndroidProjects()
        {
            var isValidName = !AndroidResourceNameHelper.ResourceNameRegex.IsMatch(ResourceName);
            if (isValidName && TargetProjects.Any(p => p.IsIncluded && p.Project.IsAndroidProject()))
            {
                return false;
            }
            return true;
        }

        async Task ShowLicensePromptAsync()
        {
            var summary = "The Image Importing is only available in MFractor Professional.";
            var operations = GetIncludedProjectsImportOperations();

            summary += Environment.NewLine;
            summary += "In MFractor Professional, the Image Importer would have created the following files:";

            foreach (var o in operations)
            {
                summary += Environment.NewLine + o.TargetProject.Name + ":";

                foreach (var d in o.Densities)
                {
                    var virtualPath = ImageDownsamplingHelper.GetVirtualFilePath(o, d);

                    summary += Environment.NewLine + " - " + virtualPath;

                    var newSize = ImageDownsamplingHelper.GetTransformedImageSize(o.SourceSize,
                                                                                    d.Scale,
                                                                                    o.SourceDensity.Scale);

                    summary += $"({newSize.Width}w by {newSize.Height}h)";
                }
            }

            await workEngine.ApplyAsync(new RequestTrialPromptWorkUnit("The Image Importing is only available in MFractor Professional.", summary, "Image Import Wizard"));
        }
    }
}
