using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Configuration;
using MFractor.Images.Importing;
using MFractor.Images.Models;
using MFractor.IOC;
using MFractor.Utilities;
using MFractor.Views.Branding;
using MFractor.Views.Controls;
using Microsoft.CodeAnalysis;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.AppIconImporter
{
    public class AppIconImporterDialog : Dialog
    {
        const string lastFolderSelection = "com.mfractor.icon_wizard.last_folder_selection";
        const string adaptiveIconPreference = "com.mfractor.icon_wizard.adaptive_icon";
        const int defaultSpacing = 8;

        Lazy<IIconImporterService> iconImporterServiceHolder = new Lazy<IIconImporterService>(() => Resolver.Resolve<IIconImporterService>());
        IIconImporterService IconImporterService => iconImporterServiceHolder.Value;

        [Import]
        IUserOptions UserOptions { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }

        [Import]
        IImageUtilities ImageSizeUtils { get; set; }

        AppIconImport operation;

        VBox root;
        ImageView imageFileErrorImage;
        TextEntry imageFilePathEntry;
        Button selectImageButton;
        Button importButton;
        SelectTargetProjectsList targetProjectsList;
        AppIconImagePreview imagePreview;
        CheckBox cleanupCheckBox;
        LinkLabel cleanupLinkLabel;

        CheckBox createAdaptiveIcons;
        LinkLabel createAdaptiveIconsHelp;

        BrandedFooter brandedFooter;

        public AppIconImporterDialog(IReadOnlyList<Project> projects)
        {
            Resolver.ComposeParts(this);

            operation = new AppIconImport(projects, ImageSizeUtils);
            operation.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AppIconImport.ImageFilePath))
                {
                    imageFilePathEntry.Text = operation.ImageFilePath;
                }
                if (e.PropertyName == nameof(AppIconImport.IsValidImageFilePath))
                {
                    imageFileErrorImage.Visible = !operation.IsValidImageFilePath;
                }
            };

            Title = "App Icon Importer";
            Icon = Image.FromResource("mfractor_logo.png");

            Width = 800;
            Height = 450;

            targetProjectsList = new SelectTargetProjectsList(operation.Projects);
            Build();
        }

        void Build()
        {
            root = new VBox();
            importButton = new Button
            {
                HeightRequest = 40,
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                Label = "Import Icon",
                TooltipText = "Imports the icon into the selected projects, generating the required versions.",
                ImagePosition = ContentPosition.Center,
            };
            importButton.Clicked += async delegate
            {
                //var isCleanup = cleanupCheckBox.Active;
                await RunImport();
            };
            imagePreview = new AppIconImagePreview(ImageSizeUtils);
            brandedFooter = new BrandedFooter("https://docs.mfractor.com", "AppIconImporter");

            root.PackStart(BuildImageSelection());
            root.PackStart(BuildContentArea(), true, true);
            root.PackStart(importButton);
            root.PackStart(new HSeparator());
            root.PackStart(brandedFooter);

            Content = root;
        }

        /// <summary>
        /// The content area is the Middle section of the UI.
        /// On this initial version it will contains the list of projects (on the left) and
        /// the preview pane (on the right).
        /// In the next version we plan to redesign this screen to include the
        /// Idioms preview selection.
        /// </summary>
        /// <returns>A Widget for the Content Area of the window.</returns>
        Widget BuildContentArea()
        {
            var container = new HBox();

            cleanupCheckBox = new CheckBox
            {
                Font = Font.SystemFont.WithSize(16).WithWeight(FontWeight.Bold),
                Label = "Cleanup existing icons",
                TooltipText = "Check this to overwrite any existing image files with the same name.",
                Active = true,
            };
            cleanupLinkLabel = new LinkLabel("How it Works?");
            cleanupLinkLabel.NavigateToUrl += (sender, e) =>
            {
                e.SetHandled();
                var launcher = Resolver.Resolve<IUrlLauncher>();
                launcher.OpenUrl("https://docs.mfractor.com/image-management/app-icon-importer/#cleanup-existing-icons");
            };

            createAdaptiveIcons = new CheckBox()
            {
                Font = Font.SystemFont.WithSize(16).WithWeight(FontWeight.Bold),
                Label = "Create Adaptive Icon",
                TooltipText = "When creating icons for Android projects, should MFractor also generate an adaptive icon configuration",
                Active = true,
            };
            createAdaptiveIcons.Active = UserOptions.Get(adaptiveIconPreference, true);
            createAdaptiveIcons.Clicked += CreateAdaptiveIcons_Clicked;

            createAdaptiveIconsHelp = new LinkLabel("Learn More");
            createAdaptiveIconsHelp.NavigateToUrl += (sender, e) =>
            {
                e.SetHandled();
                var launcher = Resolver.Resolve<IUrlLauncher>();
                launcher.OpenUrl("https://developer.android.com/guide/practices/ui_guidelines/icon_design_adaptive", false);
            };

            var targetProjectsLabel = new Label()
                .SetTitleFont()
                .WithText("Target Projects");

            var leftContainer = new VBox();
            leftContainer.PackStart(targetProjectsLabel);
            leftContainer.PackStart(targetProjectsList, true, true);

            //var cleanupContainer = new HBox();
            //cleanupContainer.PackStart(cleanupCheckBox);
            //cleanupContainer.PackStart(cleanupLinkLabel);
            //leftContainer.PackStart(cleanupContainer);

            var adaptiveIconsContainer = new HBox();
            adaptiveIconsContainer.PackStart(createAdaptiveIcons);
            adaptiveIconsContainer.PackStart(createAdaptiveIconsHelp);
            leftContainer.PackStart(adaptiveIconsContainer);

            container.PackStart(leftContainer, true, true);
            container.PackStart(new VSeparator { MarginLeft = defaultSpacing, MarginRight = defaultSpacing });
            container.PackStart(imagePreview);

            return container;
        }

        private void CreateAdaptiveIcons_Clicked(object sender, EventArgs e)
        {
            UserOptions.Set(adaptiveIconPreference, createAdaptiveIcons.Active);
        }

        HBox BuildImageSelection()
        {
            var container = new HBox();
            var targetLabel = new Label
            {
                Text = "Source Image Path: ",
                WidthRequest = 110,
            };

            container.PackStart(targetLabel);

            imageFileErrorImage = new ImageView
            {
                Image = Image.FromResource("exclamation.png").WithSize(4.5, 15.5),
                TooltipText = "Choose an image file (png, jpg or jpeg) to import.",
                WidthRequest = 15,
            };
            container.PackStart(imageFileErrorImage);

            imageFilePathEntry = new TextEntry
            {
                ReadOnly = true,
            };
            container.PackStart(imageFilePathEntry, true, true);

            selectImageButton = new Button("Choose Image");
            selectImageButton.Clicked += (sender, e) =>
            {
                ChooseImage();
            };
            container.PackStart(selectImageButton);

            return container;
        }

        void ChooseImage()
        {
            var imageFilePath = string.Empty;
            var folder = UserOptions.Get(lastFolderSelection, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            var imageFilter = new FileDialogFilter("Images", "*.png", "*.PNG", "*.jpg", "*.JPG", "*.jpeg", "*.JPEG");

            using var chooser = new OpenFileDialog("Choose an image for the icon to import");
            chooser.Filters.Add(imageFilter);
            chooser.ActiveFilter = imageFilter;
            chooser.Multiselect = false;
            chooser.CurrentFolder = folder;

            var result = chooser.Run(this);

            if (result)
            {
                imageFilePath = chooser.FileName;
                if (!ImageHelper.IsImageFile(imageFilePath))
                {
                    DialogsService.ShowError($"The file '{imageFilePath}' is not an image file.");
                    return;
                }
            }

            SetImageSelection(imageFilePath);
        }

        void SetInteractionEnabled(bool isEnabled)
        {
            selectImageButton.Sensitive = isEnabled;
            importButton.Sensitive = isEnabled;
        }

        void SetInProgress(bool inProgress)
        {
            brandedFooter.IsInProgress = inProgress;
        }

        void SetImageSelection(string imageFilePath)
        {
            operation.ImageFilePath = imageFilePath;
            imagePreview.SetImage(imageFilePath);
        }

        async Task RunImport()
        {
            SetInteractionEnabled(false);
            if (!operation.IsValid)
            {
                DialogsService.ShowError(operation.ValidationErrors);
                SetInteractionEnabled(true);
                return;
            }

            if (!operation.IsMinimumSize)
            {
                var question = new Question("The source image has a size less than 1024x1024, which is the recommended size to avoid upscaling the image on some of the imported icons. Are you sure you want to continue with this image?", "Minimum Recommended Size");
                var result = await DialogsService.AskQuestionAsync(question, "OK", "Cancel");
                if (result == "Cancel")
                {
                    return;
                }
            }

            var imageFilePath = operation.ImageFilePath;
            var targetList = operation.Projects
                .Where(t => t.IsSelected)
                .Select(t => new AppIconImportTarget(t.Project, createAdaptiveIcons.Active))
                .ToList();

            SetInProgress(true);
            try
            {
                foreach (var target in targetList)
                {
                    //if (isCleanup)
                    //{
                    //    await IconImporterService.CleanupAppIconAsync(target.TargetProject);
                    //}
                    var icons = target.Icons;
                    await IconImporterService.ImportIconAsync(icons, imageFilePath, target.TargetProject);
                }
            }
            finally
            {
                SetInProgress(false);
            }

            //DialogsService.ShowMessage("The icons has been imported to selected projects.", "OK");
            Close();
        }
    }
}
