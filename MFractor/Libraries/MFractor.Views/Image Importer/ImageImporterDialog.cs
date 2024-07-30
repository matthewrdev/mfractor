using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Android.Helpers;
using MFractor.Configuration;

using MFractor.Images;
using MFractor.Images.Importing;
using MFractor.Images.Settings;
using MFractor.Images.Utilities;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Progress;
using MFractor.Utilities;
using MFractor.Views.Branding;
using MFractor.Views.ImageImporter.Preview;
using MFractor.Views.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.ImageImporter
{
    public class ImageImporterDialog : Dialog, IAnalyticsFeature
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public string AnalyticsEvent => "Image Import Wizard";

        const string lastFolderSelection = "com.mfractor.image_wizard.last_folder_selection";

        [Import]
        IWorkEngine WorkEngine { get; set; }

        [Import]
        ILicensingService LicensingService { get; set; }

        [Import]
        IUserOptions UserOptions { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }

        [Import]
        IImageUtilities ImageUtil { get; set; }

        [Import]
        IImageImporterService ImageImporterService { get; set; }

        [Import]
        IImageFeatureSettings ImageFeatureSettings { get; set; }

        [Import]
        IDispatcher Dispatcher { get; set; }

        VBox container;
        HBox content;

        VBox leftContainer;
        VBox rightContainer;

        HBox imageSelectionContainer;
        Label imageTargetLabel;
        TextEntry imageFilePathEntry;
        ImageView imageFileErrorImage;

        HBox imageNameContainer;
        Label imageNameLabel;
        TextEntry imageNameEntry;
        ImageView imageNameErrorImage;
        ImageView imageNameWarningImage;
        Button fixImageNameButton;

        ComboBox iosImageTypeComboBox;
        ComboBox androidImageTypeComboBox;

        ComboBox iosImageDensitiesComboBox;
        ComboBox androidImageDensitiesComboBox;

        Button selectImageButton;

        HBox choicesTitleContainer;
        ImageView choicesErrorImage;
        Label androidLable;

        ListView listView;
        ListStore listDataStore;
        DataField<bool> includeProjectField;
        DataField<string> projectNameField;
        DataField<string> folderPathField;

        FrameBox imageFrame;
        SourceImage sourceImage;
        ImageSize imageSize;
        ImageView imagePreview;

        Label inputSizeLabel;

        VBox resizeImageContainer;

        CheckBox resizeCheckBox;

        HBox newHeightContainer;
        ImageView newHeightErrorImage;
        Label newHeightLabel;
        TextEntry newHeightEntry;

        HBox newWidthContainer;
        ImageView newWidthErrorImage;
        Label newWidthLabel;
        TextEntry newWidthEntry;

        ImageImportOperationPreviewControl previewControl;

        HSeparator progressSeparator;
        Progress.ProgressMonitorControl progressMonitor;

        Button importImageButton;
        private VBox densitiesContainer;
        readonly List<ImageImportTargetProject> projects;

        public bool AllowMultipleImports { get; internal set; } = true;
        public Project SelectedProject { get; private set; }

        public event EventHandler<ImportImageEventArgs> OnImageImported;

        public ImageImporterDialog(List<Project> projects, string imageName = "")
            : this(projects.Select(p => new ProjectSelection(p, true)).ToList(), imageName)
        {
        }

        public ImageImporterDialog(List<ProjectSelection> projects, string imageName = "")
            : this(projects.Select(p => new ImageImportTargetProject(p.Project, p.SelectedByDefault)).ToList(), imageName)
        {
        }

        public ImageImporterDialog(List<ImageImportTargetProject> projects, string imageName = "")
        {
            Resolver.ComposeParts(this);

            Title = "Import Image Asset";
            Icon = Image.FromResource("mfractor_logo.png");

            Size = new Size(960, 640);
            this.projects = projects;
            Build();

            SetImageName(imageName);
            Validate();
        }

        void Build()
        {
            content = new HBox();
            content.SetDragSource(TransferDataType.Uri);
            content.DragDrop += RootContainer_DragDrop;

            BuildLeftPanel();
            BuildRightPanel();

            content.PackStart(leftContainer, true);
            content.PackStart(new VSeparator());
            content.PackStart(rightContainer, false);

            container = new VBox();
            container.PackStart(content, true, true);

            BuildImagePreviewPanel();

            BuildProgressMonitor();

            importImageButton = new Button()
            {
                HeightRequest = 40,
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                Label = "Import Image",
                TooltipText = "Imports the image into the selected projects, generating down-scaled version of each image according to the selected image sizes",
                ImagePosition = ContentPosition.Center,
            };

            importImageButton.Clicked += async delegate
            {
                await ImportImage();
            };

            container.PackStart(importImageButton);

            container.PackStart(new HSeparator());
            container.PackStart(new BrandedFooter("https://docs.mfractor.com/image-management/image-importer/"));

            Content = container;
        }

        void BuildProgressMonitor()
        {
            progressSeparator = new HSeparator();
            progressMonitor = new ProgressMonitorControl();

            container.PackStart(progressSeparator);
            container.PackStart(progressMonitor);

            progressSeparator.Visible = false;
            progressMonitor.Visible = false;
        }

        public ImageImporterDialog(Solution solution)
            : this(solution.GetMobileProjects())
        {
        }

        void BuildImagePreviewPanel()
        {
            previewControl = new ImageImportOperationPreviewControl();
            container.PackStart(previewControl);
        }

        void RootContainer_DragDrop(object sender, DragEventArgs e)
        {
#pragma warning disable IDE0022 // Use expression body for methods
            e.Success = true;
#pragma warning restore IDE0022 // Use expression body for methods
        }

        void BuildLeftPanel()
        {
            leftContainer = new VBox();

            imageSelectionContainer = new HBox();

            imageTargetLabel = new Label()
            {
                Text = "File Path: ",
                WidthRequest = 110,
            };

            imageSelectionContainer.PackStart(imageTargetLabel);

            imageFileErrorImage = new ImageView
            {
                Image = Image.FromResource("exclamation.png").WithSize(4.5, 15.5),
                TooltipText = "Choose an image file (png, jpg or jpeg) to import.",
                WidthRequest = 15
            };
            imageSelectionContainer.PackStart(imageFileErrorImage);

            imageFilePathEntry = new TextEntry()
            {
                ReadOnly = true,
            };
            imageFilePathEntry.Changed += (sender, e) =>
            {
                ValidateInBackground();
            };

            imageSelectionContainer.PackStart(imageFilePathEntry, true, true);

            selectImageButton = new Button("Choose Image");
            selectImageButton.Clicked += (sender, e) =>
            {
                ChooseImage();
            };
            imageSelectionContainer.PackStart(selectImageButton);

            leftContainer.PackStart(imageSelectionContainer);

            imageNameContainer = new HBox();

            imageNameLabel = new Label("Resource Name: ")
            {
                WidthRequest = 110,
            };

            imageNameContainer.PackStart(imageNameLabel);

            imageNameErrorImage = new ImageView
            {
                Image = Image.FromResource("exclamation.png").WithSize(4.5, 15.5),
                WidthRequest = 15,
                TooltipText = "Enter a name for the new image resource."
            };
            imageNameContainer.PackStart(imageNameErrorImage);

            imageNameWarningImage = new ImageView
            {
                Image = Image.FromResource("exclamation.png").WithSize(4.5, 15.5),
                WidthRequest = 15,
                TooltipText = "The name of this resource does not comply with the Androids resource naming restrictions (valid characters are 0-9, a-z, A-Z, '.' and '_').\n\nImporting the new image with this name will cause build errors in any Android projects.\n\nPlease use the cleanup image name wand to convert the image name into a valid Android resource name."
            };
            imageNameContainer.PackStart(imageNameWarningImage);

            imageNameEntry = new TextEntry();
            imageNameEntry.Changed += (sender, e) =>
            {
                ValidateInBackground();
                previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
            };

            imageNameContainer.PackStart(imageNameEntry, true, true);

            fixImageNameButton = new Button();
            fixImageNameButton.Image = Image.FromResource("wand.png").WithSize(20, 20);
            fixImageNameButton.VerticalPlacement = WidgetPlacement.Center;
            fixImageNameButton.HorizontalPlacement = WidgetPlacement.Center;
            fixImageNameButton.TooltipText = "Converts the image name to be compatible with Androids resource naming restrictions.\n\nAndroid resource names must contain only the following characters; 0-9, a-z, A-Z, '.' and '_'.\n\nThis operation removes all characters that are invalid within an Android resource name.";
            fixImageNameButton.Clicked += (sender, e) =>
            {
                var validName = string.Join("", imageNameEntry.Text.Select(c => AndroidResourceNameHelper.ResourceNameRegex.IsMatch(c.ToString()) ? c.ToString() : ""));
                imageNameEntry.Text = validName;
                ValidateInBackground();
            };

            imageNameContainer.PackStart(fixImageNameButton, false);

            leftContainer.PackStart(imageNameContainer);

            leftContainer.PackStart(new HSeparator());

            BuildListTitle();

            BuildDensitiesAndTypeContainer();

            BuildListView();
        }

        void BuildListTitle()
        {
            choicesTitleContainer = new HBox();

            androidLable = new Label()
            {
                Text = "Target Projects",
                HeightRequest = 30,
                Font = Font.SystemFont.WithSize(20).WithWeight(FontWeight.Bold),
            };
            choicesTitleContainer.PackStart(androidLable);

            choicesErrorImage = new ImageView
            {
                Image = Image.FromResource("exclamation.png").WithSize(4.5, 15.5),
                TooltipText = "Choose one or more target projects that the new image will be imported into.",
                WidthRequest = 15
            };
            choicesTitleContainer.PackStart(choicesErrorImage);

            leftContainer.PackStart(choicesTitleContainer);
        }

        void BuildDensitiesAndTypeContainer()
        {
            densitiesContainer = new VBox();

            var androidContainer = new HBox();
            var iosContainer = new HBox();

            // Android - [Kind] - [Density]
            // iOS - [Kind] - [Density]

            var androidLabel = new Label()
            {
                Text = "Android:",
                HeightRequest = 24,
                Font = Font.SystemFont.WithSize(20).WithWeight(FontWeight.Bold),
            };
            androidContainer.PackStart(androidLabel);

            androidImageTypeComboBox = new ComboBox();
            androidImageTypeComboBox.Items.Add(ImageResourceType.Drawable, "Drawable");
            androidImageTypeComboBox.Items.Add(ImageResourceType.MipMap, "Mip Map");
            androidImageTypeComboBox.SelectedItem = ImageFeatureSettings.DefaultAndroidResourceType;
            androidImageTypeComboBox.SelectionChanged += AndroidImageTypeComboBox_SelectionChanged;

            androidContainer.PackStart(androidImageTypeComboBox);

            androidImageDensitiesComboBox = new ComboBox();
            foreach (var density in ImageDensityHelper.BuildAndroidImageDensities())
            {
                androidImageDensitiesComboBox.Items.Add(density, density.Name);
            }
            androidImageDensitiesComboBox.SelectedIndex = androidImageDensitiesComboBox.Items.Count - 1;
            androidImageDensitiesComboBox.SelectionChanged += AndroidImageDensitiesComboBox_SelectionChanged;

            androidContainer.PackStart(androidImageDensitiesComboBox);

            var iosLabel = new Label()
            {
                Text = "iOS:",
                HeightRequest = 24,
                Font = Font.SystemFont.WithSize(20).WithWeight(FontWeight.Bold),
            };
            iosContainer.PackStart(iosLabel);

            iosImageTypeComboBox = new ComboBox();
            iosImageTypeComboBox.Items.Add(ImageResourceType.BundleResource, "Bundle Resource");
            iosImageTypeComboBox.Items.Add(ImageResourceType.AssetCatalog, "Asset Catalog");
            iosImageTypeComboBox.SelectedItem = ImageFeatureSettings.DefaultIOSResourceType;
            iosImageTypeComboBox.SelectionChanged += IosImageTypeComboBox_SelectionChanged;

            iosContainer.PackStart(iosImageTypeComboBox);

            iosImageDensitiesComboBox = new ComboBox();
            foreach (var density in ImageDensityHelper.BuildAppleUnifiedImageDensities())
            {
                iosImageDensitiesComboBox.Items.Add(density, density.Name);
            }
            iosImageDensitiesComboBox.SelectedIndex = iosImageDensitiesComboBox.Items.Count - 1;
            iosImageDensitiesComboBox.SelectionChanged += IosImageDensitiesComboBox_SelectionChanged;

            iosContainer.PackStart(iosImageDensitiesComboBox);

            densitiesContainer.PackStart(androidContainer);
            densitiesContainer.PackStart(iosContainer);

            leftContainer.PackStart(densitiesContainer);
        }

        private async void IosImageDensitiesComboBox_SelectionChanged(object sender, EventArgs e)
        {
            await Task.Delay(5); // Hack to ensure that the preview panel updates.

            previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
        }

        private async void IosImageTypeComboBox_SelectionChanged(object sender, EventArgs e)
        {
            ImageFeatureSettings.DefaultIOSResourceType = (ImageResourceType)iosImageTypeComboBox.SelectedItem;

            await Task.Delay(5); // Hack to ensure that the preview panel updates.

            previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
        }

        private async void AndroidImageDensitiesComboBox_SelectionChanged(object sender, EventArgs e)
        {
            await Task.Delay(5); // Hack to ensure that the preview panel updates.

            previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
        }

        private async void AndroidImageTypeComboBox_SelectionChanged(object sender, EventArgs e)
        {
            ImageFeatureSettings.DefaultAndroidResourceType = (ImageResourceType)androidImageTypeComboBox.SelectedItem;

            await Task.Delay(5); // Hack to ensure that the preview panel updates.
            
            previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
        }

        bool ValidateProjectChoices()
        {
            var operations = GetChosenDownsampleOperations();

            var valid = true;
            if (operations == null || !operations.Any())
            {
                valid = false;
            }

            choicesErrorImage.Visible = !valid;

            return valid;
        }

        bool ValidateImageName()
        {
            var valid = true;

            fixImageNameButton.Visible = false;
            imageNameWarningImage.Visible = false;
            imageNameErrorImage.Visible = false;

            if (string.IsNullOrEmpty(imageNameEntry.Text))
            {
                valid = false;
                imageNameErrorImage.Visible = true;
            }
            else if (!AndroidResourceNameHelper.ResourceNameRegex.IsMatch(imageNameEntry.Text))
            {
                imageNameWarningImage.Visible = true;
                fixImageNameButton.Visible = true;
            }

            return valid;
        }

        bool ValidateImageFilePath()
        {
            var valid = true;

            if (string.IsNullOrEmpty(imageFilePathEntry.Text))
            {
                valid = false;
            }

            imageFileErrorImage.Visible = !valid;

            return valid;
        }

        bool ChooseImage()
        {
            var folder = UserOptions.Get(lastFolderSelection, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            var imageFilter = new FileDialogFilter("Images", "*.png", "*.PNG", "*.jpg", "*.JPG", "*.jpeg", "*.JPEG");

            var chooser = new Xwt.OpenFileDialog("Choose an image to import");
            chooser.Filters.Add(imageFilter);
            chooser.ActiveFilter = imageFilter;
            chooser.Multiselect = false;
            chooser.CurrentFolder = folder;

            var result = chooser.Run(this);

            var imageFilePath = "";

            try
            {
                if (result)
                {
                    imageFilePath = chooser.FileName;

                    if (!ImageHelper.IsImageFile(imageFilePath))
                    {
                        DialogsService.ShowError($"The file '{imageFilePath}' is not an image file.");
                        return false;
                    }
                }
            }
            finally
            {
                chooser.Dispose();
            }

            if (!SetImage(imageFilePath, false))
            {
                return false;
            }

            if (string.IsNullOrEmpty(imageFilePath))
            {
                return false;
            }

            var fileInfo = new FileInfo(imageFilePath);
            UserOptions.Set(lastFolderSelection, fileInfo.Directory.FullName);

            imageFilePathEntry.Text = imageFilePath;
            imagePreview.Image = Image.FromFile(imageFilePath).WithBoxSize(250, 220);

            imageSize = ImageUtil.GetImageSize(imageFilePath);
            inputSizeLabel.Text = "Width: " + imageSize.Width.ToString() + " | Height: " + imageSize.Height.ToString();

            UnbindImageSizeEvents();

            resizeCheckBox.Active = false;
            resizeCheckBox.Visible = true;
            newWidthContainer.Visible = true;
            newHeightContainer.Visible = true;

            try
            {
                if (string.IsNullOrEmpty(newHeightEntry.Text))
                {
                    newHeightEntry.Text = imageSize.Height.ToString();
                }
                else
                {
                    resizeCheckBox.Active = true;
                }

                if (string.IsNullOrEmpty(newWidthEntry.Text))
                {
                    newWidthEntry.Text = imageSize.Width.ToString();
                }
                else
                {
                    resizeCheckBox.Active = true;
                }

            }
            finally
            {
                BindImageSizeEvents();
            }

            if (string.IsNullOrEmpty(imageNameEntry.Text))
            {
                SetImageName(imageFilePath);
            }

            Validate();
            return true;
        }

        public bool SetImage(string filePath, bool preserveImageDimensions)
        {
            imageFilePathEntry.Text = filePath;

            UnbindImageSizeEvents();

            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    imagePreview.Image = Image.FromFile(filePath).WithBoxSize(250, 220);

                    sourceImage = new SourceImage(filePath);

                    imageSize = ImageUtil.GetImageSize(filePath);
                    inputSizeLabel.Text = "Width: " + imageSize.Width.ToString() + " | Height: " + imageSize.Height.ToString();

                    if (!resizeCheckBox.Active || !preserveImageDimensions)
                    {
                        newHeightEntry.Text = imageSize.Height.ToString();
                        newWidthEntry.Text = imageSize.Width.ToString();
                    }

                    if (string.IsNullOrEmpty(imageNameEntry.Text))
                    {
                        SetImageName(filePath);
                    }

                    if (SelectedProject == null && projects.Any() && listView.DataSource.RowCount > 0)
                    {
                        listView.SelectionChanged -= ListView_SelectionChanged;
                        listView.SelectRow(0);
                        listView.SelectionChanged += ListView_SelectionChanged;
                        SelectedProject = listView.SelectedRow < projects.Count ? projects[listView.SelectedRow].Project : null;
                    }

                    previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
                }
                else
                {
                    sourceImage = null;
                    imageSize = null;
                    imagePreview.Image = Image.FromResource("mfractor_logo_grayscale.png").WithBoxSize(250, 220);

                    if (!preserveImageDimensions)
                    {
                        newHeightEntry.Text = string.Empty;
                        newWidthEntry.Text = string.Empty;
                    }

                    imageNameEntry.Text = string.Empty;

                    previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
                }
            }
            finally
            {
                BindImageSizeEvents();
            }

            Validate();

            return true;
        }

        void SetImageName(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var fileInfo = new FileInfo(filePath);

                var name = Path.GetFileName(Path.GetFileNameWithoutExtension(filePath));
                var extension = Path.GetExtension(filePath);

                if (name.EndsWith("@2x", StringComparison.OrdinalIgnoreCase) || name.EndsWith("@3x", StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - "@2x".Length);
                }

                imageNameEntry.Text = name + extension;

            }
            else
            {
                imageNameEntry.Text = string.Empty;
            }
        }

        void BuildListView()
        {
            listView = new ListView();

            includeProjectField = new DataField<bool>();
            projectNameField = new DataField<string>();

            folderPathField = new DataField<string>();

            listDataStore = new ListStore(includeProjectField, projectNameField, folderPathField);
            listView.DataSource = listDataStore;
            listView.GridLinesVisible = GridLines.Horizontal;
            listView.SelectionChanged += ListView_SelectionChanged;

            var includeCell = new CheckBoxCellView { Editable = true, ActiveField = includeProjectField };
            includeCell.Toggled += async (sender, e) =>
            {
                ValidateInBackground();

                await Task.Delay(5); // Hack to ensure that the preview panel updates.

                previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
            };

            var projectNameCell = new TextCellView() { Editable = false, TextField = projectNameField };

            var folderPathCell = new TextCellView() { Editable = true, TextField = folderPathField };
            folderPathCell.TextChanged += async (sender, e) =>
            {
                await Task.Delay(5); // Hack to ensure that the preview panel updates.

                previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
            };

            listView.Columns.Add("Include", includeCell);
            listView.Columns.Add(new ListViewColumn("Project Name                  ", projectNameCell));
            //listView.Columns.Add(new ListViewColumn("Folder Path", folderPathCell));      // Temporarily Removed as of Work Item #20

            listView.Columns[1].CanResize = true;

            for (var i = 0; i < projects.Count; ++i)
            {
                var project = projects[i];

                var row = listDataStore.AddRow();
                listDataStore.SetValue(row, includeProjectField, project.Selected);
                listDataStore.SetValue(row, projectNameField, project.Project.Name);

                var imageTypes = new ItemCollection();
                foreach (var item in project.ImageTypes)
                {
                    imageTypes.Add(item, EnumHelper.GetEnumDescription(item));
                }
            }

            leftContainer.PackStart(listView, true, true);
        }

        void ListView_SelectionChanged(object sender, EventArgs e)
        {
            SelectedProject = listView.SelectedRow < projects.Count ? projects[listView.SelectedRow].Project : null;

            previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
        }

        bool Validate()
        {
            var enabled = true;

            enabled &= ValidateImageName();
            enabled &= ValidateImageFilePath();
            enabled &= ValidateProjectChoices();
            enabled &= ValidateNewHeight();
            enabled &= ValidateNewWidth();

            importImageButton.Sensitive = enabled;

            return enabled;
        }

        bool ValidateNewWidth()
        {
            if (!resizeCheckBox.Active)
            {
                newWidthErrorImage.Visible = false;
                return true;
            }

            var valid = true;

            if (string.IsNullOrEmpty(newWidthEntry.Text))
            {
                valid = false;
                newWidthErrorImage.TooltipText = "You must provide a width for the new image.";
            }
            else
            {

                if (!int.TryParse(newWidthEntry.Text, out var newWidth) && newWidthEntry.Text != "NA")
                {
                    valid = false;
                    newWidthErrorImage.TooltipText = "The specified width is not a valid number.";
                }

                if (valid && newWidthEntry.Text != "NA")
                {
                    if (newWidth <= 0)
                    {
                        valid = false;
                        newWidthErrorImage.TooltipText = "The specified width cannot be less than 1 pixel.";
                    }
                    if (imageSize != null && imageSize.Width < newWidth)
                    {
                        valid = false;
                        newWidthErrorImage.TooltipText = "The new width cannot be more than the original images width.";
                    }
                }
            }

            newWidthErrorImage.Visible = !valid;

            return valid;
        }

        bool ValidateNewHeight()
        {
            if (!resizeCheckBox.Active)
            {
                newHeightErrorImage.Visible = false;
                return true;
            }

            var valid = true;
            if (string.IsNullOrEmpty(newHeightEntry.Text))
            {
                valid = false;
                newHeightErrorImage.TooltipText = "You must provide a height for the new image.";
            }
            else
            {
                if (!int.TryParse(newHeightEntry.Text, out var newHeight) && newHeightEntry.Text != "NA")
                {
                    valid = false;
                    newHeightErrorImage.TooltipText = "The specified height is not a valid number";
                }

                if (valid
                    && newHeightEntry.Text != "NA")
                {
                    if (newHeight <= 0)
                    {
                        valid = false;
                        newHeightErrorImage.TooltipText = "The specified height cannot be less than 1 pixel.";
                    }

                    if (imageSize != null && imageSize.Height < newHeight)
                    {
                        valid = false;
                        newHeightErrorImage.TooltipText = "The new height cannot be more than the original images height.";
                    }
                }
            }

            newHeightErrorImage.Visible = !valid;

            return valid;
        }

        void ValidateInBackground()
        {
            Task.Run(async () =>
            {
                await Task.Delay(30);

                Dispatcher.InvokeOnMainThread(() =>
                {
                    Validate();
                });
            });
        }

        void BuildRightPanel()
        {
            rightContainer = new VBox
            {
                WidthRequest = 320,
                Spacing = 2,
            };

            imagePreview = new ImageView
            {
                Image = Image.FromResource("mfractor_logo_grayscale.png").WithBoxSize(250, 220)
            };

            imageFrame = new FrameBox()
            {
                HeightRequest = 220,
                WidthRequest = 280,
                VerticalPlacement = WidgetPlacement.Center,
                HorizontalPlacement = WidgetPlacement.Center,
                Content = imagePreview,
                BorderColor = new Color(.2, .2, .2),
                BorderWidth = 1,
            };

            rightContainer.PackStart(imageFrame, true, true);

            inputSizeLabel = new Label()
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                TextAlignment = Alignment.Center,
                Text = "Width: NA | Height: NA",
            };

            rightContainer.PackStart(inputSizeLabel);

            BuildResizeImageControl();

            rightContainer.PackStart(new HSeparator());
        }

        void BuildResizeImageControl()
        {
            resizeImageContainer = new VBox();

            resizeCheckBox = new CheckBox()
            {
                Font = Font.SystemFont.WithSize(16).WithWeight(FontWeight.Bold),
                Label = "Resize Image?",
                TooltipText = "Check this to change the width and height of your source image before it is used to generate the new images.",
                Active = false,
            };

            resizeCheckBox.Toggled += (sender, e) =>
            {
                UpdateResizeEntries();
                ValidateInBackground();
            };

            resizeImageContainer.PackStart(resizeCheckBox);

            newHeightContainer = new HBox();

            newHeightLabel = new Label()
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                TextAlignment = Alignment.Center,
                Text = "Height: ",
                TooltipText = "The new height of the image in pixels.",
            };

            newHeightEntry = new TextEntry();

            newHeightErrorImage = new ImageView
            {
                Image = Image.FromResource("exclamation.png").WithSize(4.5, 15.5),
                WidthRequest = 15,
                Visible = false
            };

            newHeightContainer.PackStart(newHeightErrorImage, false, false);
            newHeightContainer.PackStart(newHeightLabel, false, false);
            newHeightContainer.PackStart(newHeightEntry, true, true);

            resizeImageContainer.PackStart(newHeightContainer);

            newWidthLabel = new Label()
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                TextAlignment = Alignment.Center,
                Text = "Width:  ",
                TooltipText = "The new width of the image in pixels.",
            };

            newWidthContainer = new HBox();
            newWidthEntry = new TextEntry();

            newWidthErrorImage = new ImageView
            {
                Image = Image.FromResource("exclamation.png").WithSize(4.5, 15.5),
                WidthRequest = 15,
                Visible = false
            };

            newWidthContainer.PackStart(newWidthErrorImage, false, false);
            newWidthContainer.PackStart(newWidthLabel, false, false);
            newWidthContainer.PackStart(newWidthEntry, true, true);

            BindImageSizeEvents();

            resizeImageContainer.PackStart(newWidthContainer);

            rightContainer.PackStart(resizeImageContainer, true, true);
        }

        void OnWidthTextInput(object sender, TextInputEventArgs e)
        {
            if (!int.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
        }

        void OnHeightTextInput(object sender, TextInputEventArgs e)
        {
            if (!int.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
        }

        void BindImageSizeEvents()
        {
            newHeightEntry.TextInput += OnHeightTextInput;
            newWidthEntry.TextInput += OnWidthTextInput;
            newHeightEntry.Changed += OnHeightTextChanged;
            newWidthEntry.Changed += OnWidthTextChanged;
        }

        void UnbindImageSizeEvents()
        {
            newHeightEntry.TextInput -= OnHeightTextInput;
            newWidthEntry.TextInput -= OnWidthTextInput;
            newHeightEntry.Changed -= OnHeightTextChanged;
            newWidthEntry.Changed -= OnWidthTextChanged;
        }

        void OnHeightTextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(newHeightEntry.Text, out var height))
            {
                return;
            }

            if (imageSize != null)
            {
                resizeCheckBox.Active = true;
                var aspect = (double)imageSize.Width / (double)imageSize.Height;
                var newWidth = (double)height * aspect;

                UnbindImageSizeEvents();

                try
                {
                    newWidthEntry.Text = ((int)Math.Round(newWidth)).ToString();
                }
                finally
                {
                    BindImageSizeEvents();
                }

                ValidateInBackground();
                previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
            }
        }

        void OnWidthTextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(newWidthEntry.Text, out var width))
            {
                return;
            }

            if (imageSize != null)
            {
                resizeCheckBox.Active = true;
                var aspect = (double)imageSize.Width / (double)imageSize.Height;
                var newHeight = (double)width / aspect;

                UnbindImageSizeEvents();

                try
                {
                    newHeightEntry.Text = ((int)Math.Round(newHeight)).ToString();
                }
                finally
                {

                    BindImageSizeEvents();
                }

                ValidateInBackground();
                previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
            }
        }

        void UpdateResizeEntries()
        {
            SetImageSizeDetails(imageSize);

            previewControl.Update(SelectedProject, sourceImage, GetChosenDownsampleOperations());
        }

        void SetImageSizeDetails(ImageSize size)
        {
            if (size != null)
            {
                UnbindImageSizeEvents();

                try
                {
                    newHeightEntry.Text = size.Height.ToString();
                    newWidthEntry.Text = size.Width.ToString();
                }
                finally
                {
                    BindImageSizeEvents();
                }
            }
            else
            {
                UnbindImageSizeEvents();

                try
                {
                    newHeightEntry.Text = newWidthEntry.Text = "NA";
                }
                finally
                {
                    BindImageSizeEvents();
                }
            }
        }

        List<ImportImageOperation> GetChosenDownsampleOperations()
        {
            var sourceSize = GetMaximumSize();


            var operations = new List<ImportImageOperation>();
            for (var row = 0; row < listDataStore.RowCount; ++row)
            {
                var include = listDataStore.GetValue(row, includeProjectField);
                if (include)
                {
                    var project = projects[row].Project;
                    var imageType = GetImageResourceTypeForProject(project);

                    var folderPath = listDataStore.GetValue(row, folderPathField);

                    var densities = GetDensitiesForProject(projects[row].Project);
                    var maxDensity = densities.OrderByDescending(d => d.Scale).FirstOrDefault();

                    var operation = new ImportImageOperation(project,
                                                            imageNameEntry.Text,
                                                            imageFilePathEntry.Text,
                                                            string.Empty,
                                                            imageType,
                                                            maxDensity,
                                                            densities,
                                                            sourceSize,
                                                            folderPath);
                    operations.Add(operation);
                }
            }

            return operations;
        }

        private List<ImageDensity> GetDensitiesForProject(Project project)
        {
            if (project.IsAndroidProject())
            {
                var selection = androidImageDensitiesComboBox.SelectedItem as ImageDensity;
                var items = androidImageDensitiesComboBox.Items.OfType<ImageDensity>().ToList();


                return items.GetRange(0, items.IndexOf(selection) + 1);
            }

            if (project.IsAppleUnifiedProject())
            {
                var selection = iosImageDensitiesComboBox.SelectedItem as ImageDensity;
                var items = iosImageDensitiesComboBox.Items.OfType<ImageDensity>().ToList();

                return items.GetRange(0, items.IndexOf(selection) + 1);
            }

            throw new ArgumentOutOfRangeException();
        }

        private ImageResourceType GetImageResourceTypeForProject(Project project)
        {
            if (project.IsAndroidProject())
            {
                return (ImageResourceType)androidImageTypeComboBox.SelectedItem;
            }
            else if (project.IsAppleUnifiedProject())
            {
                return (ImageResourceType)iosImageTypeComboBox.SelectedItem;
            }

            throw new ArgumentOutOfRangeException();
        }

        async Task ShowResultPreview(List<ImportImageOperation> operations)
        {
            var summary = "The Image Importing is only available in MFractor Professional.";

            summary += Environment.NewLine;
            summary += "In MFractor Professional, the Image Importer would have created the following files:";

            foreach (var o in operations)
            {
                summary += Environment.NewLine + o.TargetProject.Name + ":";

                foreach (var d in o.Densities)
                {
                    var virtualPath = ImageDownsamplingHelper.GetVirtualFilePath(o, d);

                    summary += Environment.NewLine + " - " + virtualPath;

                    if (o.SourceSize != null)
                    {
                        var newSize = ImageDownsamplingHelper.GetTransformedImageSize(o.SourceSize,
                                                                                      d.Scale,
                                                                                      o.SourceDensity.Scale);

                        summary += $"({newSize.Width}w by {newSize.Height}h)";
                    }
                }
            }

            await WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit("The Image Importing is only available in MFractor Professional.", summary, AnalyticsEvent));
        }

        async Task<bool> ImportImage()
        {
            if (!Validate())
            {
                return false;
            }

            var isValidName = !AndroidResourceNameHelper.ResourceNameRegex.IsMatch(imageNameEntry.Text);
            if (isValidName && projects.Any(p => p.Selected && p.Project.IsAndroidProject()))
            {
                DialogsService.ShowError($"The name \"{imageNameEntry.Text}\" violates Androids naming restrictions. Accepted characters are 0-9, a-z, A-Z, . and _.\n\n\"{imageNameEntry.Text}\" will cause build errors in Android projects.\n\nPlease choose a different name or use the clean image name wand.");
                return false;
            }

            var operations = GetChosenDownsampleOperations();

            if (!LicensingService.IsPaid)
            {
                await ShowResultPreview(operations);

                return false;
            }

            void ImportDone()
            {
                if (AllowMultipleImports)
                {
                    Dispatcher.InvokeOnMainThread(() =>
                   {
                       try
                       {
                           var discard = "Yes (Discard Resize Settings)";

                           var answer = DialogsService.AskQuestion("Would you like to import another image?", "Yes", discard, "No");

                           if (answer != "No")
                           {
                               SetImage(string.Empty, answer == "Yes");
                               ChooseImage();
                           }
                           else
                           {
                               Close();
                               Dispose();
                           }
                       }
                       catch (Exception ex)
                       {
                           log?.Exception(ex);
                       }
                   });
                }
                else
                {
                    Close();
                    Dispose();
                }
            }

            var args = new ImportImageEventArgs(imageFilePathEntry.Text,
                                                imageNameEntry.Text);

            var result = await ImageImporterService.Import(operations, new StubProgressMonitor());

            OnImageImported?.Invoke(this, args);

            ImportDone();

            return true;
        }

        ImageSize? GetMaximumSize()
        {
            if (resizeCheckBox.Active
                && int.TryParse(newWidthEntry.Text, out var width)
                && int.TryParse(newHeightEntry.Text, out var height))
            {
                return new ImageSize(width, height);
            }

            return imageSize;
        }
    }
}
