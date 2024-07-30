using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Analytics;

using MFractor.Images;
using MFractor.Images.Deletion;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Views.Branding;
using MFractor.Views.Controls;
using MFractor.Views.Controls.Collection;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.ImageDeletionTool
{
    public class DeleteImagesDialog : Dialog, IAnalyticsFeature
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        class DeleteImageAssetCollectionItem : ICollectionItem
        {
            public DeleteImageAssetCollectionItem(IProjectFile projectFile, IImageUtilities imageSizeUtilities)
            {
                ProjectFile = projectFile;

                DisplayText = ProjectFile.CompilationProject.Name + " - " + ProjectFile.VirtualPath;
                SecondaryDisplayText = imageSizeUtilities.GetImageSizeSummary(projectFile.FilePath);
            }

            public Image Icon { get; }

            public string DisplayText { get; }

            public string SecondaryDisplayText { get; }

            public string SearchText => DisplayText;

            public bool IsChecked { get; set; } = true;

            public IProjectFile ProjectFile { get; }
        }

        CollectionView collectionView;

        [Import]
        IAnalyticsService Analytics { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }

        [Import]
        IWorkEngine WorkEngine { get; set; }

        [Import]
        ILicensingService LicensingService { get; set; }

        [Import]
        IImageUtilities ImageUtil { get; set; }

        [Import]
        IImageDeletionService ImageDeletionService { get; set; }

        [Import]
        IProductInformation ProductInformation { get; set; }

        public event EventHandler<ImagesDeletedEventArgs> ImagesDeleted;

        VBox root;

        HBox container;

        VBox rightContainer;

        Button deleteButton;
        ImagePreviewControl previewControl;

        public DeleteImagesDialog(IImageAsset imageAsset)
        {
            if (imageAsset is null)
            {
                throw new ArgumentNullException(nameof(imageAsset));
            }

            Resolver.ComposeParts(this);

            Title = "Delete Image Asset";
            Icon = Image.FromResource("mfractor_logo.png");

            Build();

            ImageAsset = imageAsset;

            Populate();
        }

        void Populate()
        {
            collectionView.Items = ImageAsset.AllAssets.Select(i => new DeleteImageAssetCollectionItem(i, ImageUtil)).ToList();
        }

        public IImageAsset ImageAsset { get; }

        public string AnalyticsEvent => "Delete Image Asset";

        void Build()
        {
            root = new VBox();

            container = new HBox();

            BuildLeftPanel();

            BuildRightPanel();

            root.PackStart(container, true, true);

            root.PackStart(new HSeparator());

            deleteButton = new Button("Delete Images")
            {
                ExpandHorizontal = true,
            };

            deleteButton.Clicked += DeleteButton_Clicked;

            root.PackStart(deleteButton);

            if (!LicensingService.IsPaid)
            {
                container.PackStart(new Label("The Image Asset Deletion Tool is a Professional-only MFractor feature. Please upgrade or request a trial."));
            }

            root.PackStart(new HSeparator());
            root.PackEnd(new BrandedFooter("https://docs.mfractor.com/image-management/deleting-image-assets/", "Delete Image Asset"));

            Content = root;
        }

        void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (!LicensingService.IsPaid)
            {
                WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit("The Image Asset Deletion Tool is a Professional-only MFractor feature. Please upgrade or request a trial.", AnalyticsEvent));
                return;
            }

            try
            {
                DialogsService.StatusBarMessage($"Deleting all densities of {ImageAsset.Name}...");

                var deletionTargets = GetSelectedProjectFiles();

                var message = $"Are you sure you want to delete this image asset?";

                if (ProductInformation.Product == Product.VisualStudioWindows)
                {
                    message += "\n\nThis will cause the impacted projects to reload.";
                }

                var confirmation = DialogsService.AskQuestion(message, "Yes", "Cancel");
                if (confirmation != "Yes")
                {
                    return;
                }

                var workUnits = ImageDeletionService.Delete(deletionTargets);

                WorkEngine.ApplyAsync(workUnits).ConfigureAwait(false);

                ImagesDeleted?.Invoke(this, new ImagesDeletedEventArgs(deletionTargets));

                Analytics.Track(this);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            this.Close();
            this.Dispose();
        }

        IReadOnlyList<IProjectFile> GetSelectedProjectFiles()
        {
            return collectionView.Items.OfType<DeleteImageAssetCollectionItem>()
                                       .Where(item => item.IsChecked)
                                       .Select(item => item.ProjectFile)
                                       .ToList();
        }

        void BuildLeftPanel()
        {
            var options = new CollectionViewOptions().WithPrimaryLabelColumn("Asset")
                                                     .WithSecondaryLabelColumn("Size")
                                                     .WithSelectionCheckboxColumn("Delete?");

            collectionView = new CollectionView(options)
            {
                ExpandVertical = true,
                WidthRequest = 500,
                HeightRequest = 300,
                Title = "MFractor will delete the following image assets:"
            };

            collectionView.ItemSelected += CollectionView_ItemSelected;

            container.PackStart(collectionView);
        }

        void CollectionView_ItemSelected(object sender, CollectionViewItemSelectedEventArgs e)
        {
            if (e.CollectionItem is DeleteImageAssetCollectionItem item)
            {
                previewControl.SetImage(item.ProjectFile.FilePath);
            }
        }

        void BuildRightPanel()
        {
            rightContainer = new VBox();

            previewControl = new ImagePreviewControl();

            rightContainer.PackStart(previewControl);

            container.PackStart(rightContainer);
        }
    }
}
