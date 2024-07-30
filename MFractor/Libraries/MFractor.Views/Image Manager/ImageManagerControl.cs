using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Ide;
using MFractor.Images;
using MFractor.Images.ImageManager;
using MFractor.Images.WorkUnits;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Utilities;
using MFractor.Views.Branding;
using MFractor.Views.ContextMenu;
using MFractor.Views.Controls.Collection;
using MFractor.Views.ImageImporter;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.ImageManager
{
    public class ImageManagerControl : VBox, IImageManagerController, ICollectionViewDragOperationHandler
    {
        public const int PreviewSize = 32;

        public IEnumerable<Project> Projects
        {
            get
            {
                if (Solution == null)
                {
                    return Enumerable.Empty<Project>();
                }

                var mobileProjects = Solution.GetMobileProjects();

                return mobileProjects;
            }
        }

        public Solution Solution { get; private set; }

        public string SolutionName => Solution == null ? string.Empty : Path.GetFileName(Solution.FilePath);

        public IImageManagerOptions Options { get; private set; }

        public bool WatchWorkspace { get; }

        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Image placeholderImage = Image.FromResource("image-placeholder.png").WithSize(PreviewSize, PreviewSize);

        public IReadOnlyDictionary<string, IImageAsset> ImageAssets { get; private set; } = new Dictionary<string, IImageAsset>();

        HBox contentContainer;

        VBox leftContainer;

        Button deleteImageAssetButton;
        Button optimiseAllImageAssetsButton;
        Button refreshButton;

        CollectionView imagesCollectionView;

        VBox rightContainer;

        FrameBox imageFrame;

        ImageView imagePreview;

        Label fileSizeLabel;

        Label sizeLabel;

        ComboBox projectChooser;

        ListBox selectedImageAssetsListView;
        ListStore selectedImageAssetsListDataStore;

        DataField<string> selectedImageAssetsNameField = new DataField<string>();
        DataField<Image> selectedImageAssetsPreviewField = new DataField<Image>();
        DataField<string> selectedImageAssetsSizeSummaryField = new DataField<string>();

        Button importButton;

        IProjectFile pendingProjectFileSelection = null;
        string pendingImageAssetSelection;

        public bool IsLoading { get; private set; } = true;

        Label loadingLabel;

        HSeparator importSeparator;

        [Import]
        IWorkspaceService WorkspaceService { get; set; }

        [Import]
        IWorkEngine WorkEngine { get; set; }

        [Import]
        IProjectService ProjectService { get; set; }

        [Import]
        IAnalyticsService Analytics { get; set; }

        [Import]
        ILicensingService LicensingService { get; set; }

        [Import]
        IImageAssetService ImageAssetService { get; set; }

        [Import]
        IActiveDocument ActiveDocument { get; set; }

        [Import]
        IImageManagerCommandRepository ImageManagerCommands { get; set; }

        [Import]
        IContextMenuService ContextMenuService { get; set; }

        [Import]
        IImageUtilities ImageUtil { get; set; }

        [Import]
        IDispatcher Dispatcher { get; set; }

        public ImageManagerControl(Solution solution,
                                   IImageManagerOptions options,
                                   bool watchWorkspace = true,
                                   bool suppressPurchasePush = false)
        {
            Resolver.ComposeParts(this);

            Spacing = 1;

            if (!suppressPurchasePush)
            {
                if (!LicensingService.IsPaid)
                {
                    WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit($"The Image Asset Manager is a Professional-only MFractor feature. Please upgrade or request a trial.", "Image Manager"));
                }
            }

            Solution = solution;
            Options = options;
            WatchWorkspace = watchWorkspace;

            Build();

            BindWorkspaceEvents();

            GatherImageAssetsAsync().ConfigureAwait(false);
        }

        bool AreSame(Solution left, Solution right)
        {
            var areSame = false;

            if (left == null && right != null)
            {
                areSame = false;
            }
            else if (left != null && right == null)
            {
                areSame = false;
            }
            else if (left.Id.Id == right.Id.Id)
            {
                areSame = true;
            }

            return areSame;
        }

        public void SetSolution(Solution solution, bool force = false)
        {
            if (AreSame(solution, Solution) && !force)
            {
                return;
            }

            Solution = solution;
            GatherImageAssetsAsync().ConfigureAwait(false);
        }

        public void SetOptions(IImageManagerOptions options)
        {
            Options = options ?? ImageManagerOptions.Default;

            importSeparator.Visible = Options.AllowImport;
            importButton.Visible = Options.AllowImport;
        }

        void BindWorkspaceEvents()
        {
            UnbindWorkspaceEvents();

            if (WatchWorkspace)
            {
                WorkspaceService.WorkspaceExecutionTargetChanged += Workspace_ExecutionTargetChanged;
                WorkspaceService.SolutionClosed += WorkspaceService_SolutionClosed;
                WorkspaceService.ProjectRenamed += WorkspaceService_ProjectRenamed;
                WorkspaceService.ProjectAdded += WorkspaceService_ProjectAdded;
                WorkspaceService.ProjectRemoved += WorkspaceService_ProjectRemoved;
                WorkspaceService.FilesAddedToProject += WorkspaceService_FilesAddedToProject;
                WorkspaceService.FilesRemovedFromProject += WorkspaceService_FilesRemovedFromProject;
                WorkspaceService.FilesRenamed += WorkspaceService_FilesRenamed;
            }
        }

        void UnbindWorkspaceEvents()
        {
            WorkspaceService.WorkspaceExecutionTargetChanged -= Workspace_ExecutionTargetChanged;
            WorkspaceService.SolutionClosed -= WorkspaceService_SolutionClosed;
            WorkspaceService.ProjectRenamed -= WorkspaceService_ProjectRenamed;
            WorkspaceService.ProjectAdded -= WorkspaceService_ProjectAdded;
            WorkspaceService.ProjectRemoved -= WorkspaceService_ProjectRemoved;
            WorkspaceService.FilesAddedToProject -= WorkspaceService_FilesAddedToProject;
            WorkspaceService.FilesRemovedFromProject -= WorkspaceService_FilesRemovedFromProject;
            WorkspaceService.FilesRenamed -= WorkspaceService_FilesRenamed;
        }

        void Workspace_ExecutionTargetChanged(object sender, WorkspaceExecutionTargetChangedEventArgs e)
        {
            var solution = WorkspaceService.CurrentWorkspace.CurrentSolution;

            if (solution != this.Solution)
            {
                SetSolution(solution);
            }
        }

        void WorkspaceService_FilesRenamed(object sender, FilesRenamedEventArgs args)
        {
            var projects = args.ChangeSet.ProjectGuids.Select(guid => ProjectService.GetProject(guid)).Where(p => p != null).ToList();

            if (!Projects.Any(p => args.HasProject(p)))
            {
                return;
            }

            var shouldGather = false;

            foreach (var guid in args.ChangeSet.ProjectGuids)
            {
                if (args.GetProjectFiles(guid).Any(file => ImageHelper.IsImageFile(file.OldFilePath) || ImageHelper.IsImageFile(file.NewFilePath)))
                {
                    shouldGather = true;
                    break;
                }
            }

            if (shouldGather)
            {
                GatherImageAssetsAsync().ConfigureAwait(false);
            }
        }

        void WorkspaceService_FilesRemovedFromProject(object sender, FilesEventArgs e)
        {
            TryGatherImages(e);
        }

        void WorkspaceService_FilesAddedToProject(object sender, FilesEventArgs e)
        {
            TryGatherImages(e);
        }

        void TryGatherImages(FilesEventArgs args)
        {
            var projects = args.ProjectGuids.Select(guid => ProjectService.GetProject(guid)).Where(p => p != null).ToList();

            if (!Projects.Any(p => args.HasProject(p)))
            {
                return;
            }

            var shouldGather = false;

            foreach (var guid in args.ProjectGuids)
            {
                if (args.GetProjectFiles(guid).Any(file => ImageHelper.IsImageFile(file)))
                {
                    shouldGather = true;
                    break;
                }
            }

            if (shouldGather)
            {
                GatherImageAssetsAsync().ConfigureAwait(false);
            }
        }

        void WorkspaceService_ProjectAdded(object sender, ProjectAddedEventArgs e)
        {
            if (e.SolutionName == SolutionName)
            {
                GatherImageAssetsAsync().ConfigureAwait(false);
            }
        }

        void WorkspaceService_ProjectRenamed(object sender, ProjectRenamedEventArgs e)
        {
            if (e.SolutionName == SolutionName)
            {
                GatherImageAssetsAsync().ConfigureAwait(false);
            }
        }

        void WorkspaceService_ProjectRemoved(object sender, ProjectRemovedEventArgs e)
        {
            if (e.SolutionName == SolutionName)
            {
                GatherImageAssetsAsync().ConfigureAwait(false);
            }
        }

        void WorkspaceService_SolutionClosed(object sender, SolutionClosedEventArgs e)
        {
            if (e.SolutionName == SolutionName)
            {
                Solution = null;
                SelectedProjectFile = null;
                GatherImageAssetsAsync().ConfigureAwait(false);
                ApplyImageAssetSelectionUserInterface();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            UnbindWorkspaceEvents();
        }

        public void Select(string imageAssetName)
        {
            pendingProjectFileSelection = null;
            pendingImageAssetSelection = null;

            if (!IsLoading)
            {
                imagesCollectionView.Focus(item =>
                {
                    if (item is ImageAssetCollectionItem imageItem
                        && imageItem.ImageAsset.Name == imageAssetName)
                    {
                        return true;
                    }

                    return false;
                });
            }
            else
            {
                pendingImageAssetSelection = imageAssetName;
            }
        }

        public void Select(IProjectFile projectFile)
        {
            pendingProjectFileSelection = projectFile;
            if (!IsLoading)
            {
                var imageAssetName = ImageNameHelper.GetImageAssetName(projectFile);

                imagesCollectionView.Focus(item =>
                {
                    if (item is ImageAssetCollectionItem imageItem
                        && imageItem.ImageAsset.Name == imageAssetName)
                    {
                        return true;
                    }

                    return false;
                });
            }
        }

        void Build()
        {
            contentContainer = new HBox()
            {
                Spacing = 1,
            };

            this.Sensitive = LicensingService.IsPaid;

            BuildLeftPanel();

            contentContainer.PackStart(new VSeparator());

            BuildRightPanel();

            PackStart(contentContainer, true, true);

            loadingLabel = new Label
            {
                Text = "Please wait while the image manager gathers your image assets."
            };

            PackStart(loadingLabel);

            if (!LicensingService.IsPaid)
            {
                var paidActivation = new Label
                {
                    Text = "The Image Asset Manager is a Professional-only MFractor feature. Please upgrade or request a trial."
                };

                PackStart(paidActivation);
            }

            importButton = new Button("Import New Image Asset");
            importButton.Clicked += ImportButton_Clicked;
            importSeparator = new HSeparator();

            PackStart(importSeparator);
            PackStart(importButton);


            PackStart(new HSeparator());
            PackStart(new BrandedFooter("https://docs.mfractor.com/image-management/managing-image-assets/", "Image Manager"));

            SetOptions(Options);
        }

        void ImportButton_Clicked(object sender, EventArgs e)
        {
            WorkEngine.ApplyAsync(new ImportImageAssetWorkUnit()
            {
                Projects = this.Projects.ToList(),
                AllowMultipleImports = true,
            }).ConfigureAwait(false);
        }

        async void ImageImporter_OnImageImported(object sender, ImportImageEventArgs e)
        {
            await GatherImageAssetsAsync().ConfigureAwait(false);
        }

        public Task GatherImageAssetsAsync()
        {
            return Task.Run(async () =>
            {
                await Dispatcher.InvokeOnMainThreadAsync(() =>
                {
                    ToggleLoading(true);
                });

                ImageAssets = ImageAssetService.GatherImageAssets(Solution);

                await Dispatcher.InvokeOnMainThreadAsync(() =>
                {
                    PopulateImageListView();

                    ToggleLoading(false);
                });
            });
        }

        void ToggleLoading(bool isLoading)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                this.IsLoading = isLoading;
                loadingLabel.Visible = isLoading;
            });
        }

        void PopulateImageListView(string searchText = "")
        {
            imagesCollectionView.Items = GetCollectionItems().ToList();

            if (!string.IsNullOrEmpty(pendingImageAssetSelection))
            {
                Select(pendingImageAssetSelection);
            }
        }

        IEnumerable<ICollectionItem> GetCollectionItems()
        {
            if (this.ImageAssets == null || !ImageAssets.Any())
            {
                return Enumerable.Empty<ICollectionItem>();
            }

            return ImageAssets.Values.Select(asset => new ImageAssetCollectionItem(asset, placeholderImage));
        }

        void BuildLeftPanel()
        {
            leftContainer = new VBox
            {
                ExpandVertical = true,
                Spacing = 1,
            };

            var options = new CollectionViewOptions().WithIconColumn("Image")
                                                     .WithPrimaryLabelColumn("Name");

            imagesCollectionView = new CollectionView(options, this);
            imagesCollectionView.Title = "Image Assets";
            imagesCollectionView.SearchPlaceholderText = "Search for an image asset...";
            imagesCollectionView.ItemRightClicked += (sender, e) =>
            {
                var context = new ImageManagerCommandContext(SelectedImageAsset, null, Options, this);

                ShowContextMenu(context, e.HostWidget, e.ButtonEventArgs.X, e.ButtonEventArgs.Y);
            };

            imagesCollectionView.ItemDoubleClicked += (sender, e) =>
            {
                if (SelectedProjectFile != null)
                {
                    Process.Start(SelectedProjectFile.FilePath);
                }
            };

            imagesCollectionView.ItemSelected += (object sender, CollectionViewItemSelectedEventArgs e) =>
            {
                ApplyImageSelection(SelectedImageAsset);
            };

            imagesCollectionView.ExpandVertical = true;
            imagesCollectionView.WidthRequest = 300;
            leftContainer.PackStart(imagesCollectionView, true, true);

            deleteImageAssetButton = new Button()
            {
                Label = "Delete Selected Image Asset",
                TooltipText = "Deletes the selected image asset, removing all variants of that image asset from all projects in this solution.",
            };
            deleteImageAssetButton.Clicked += DeleteImageAssetButton_Clicked;

            leftContainer.PackStart(deleteImageAssetButton);

            optimiseAllImageAssetsButton = new Button()
            {
                Label = "Optimise All Images",
                TooltipText = "Uses TinyPNG to optimise all image assets that are present in the current solution.",
            };

            optimiseAllImageAssetsButton.Clicked += OptimiseAllImageAssetsButton_Clicked;

            leftContainer.PackStart(optimiseAllImageAssetsButton);

            refreshButton = new Button()
            {
                Label = "Refresh",
                TooltipText = "Refresh the images displayed in the image manager",
            };

            refreshButton.Clicked += RefreshClicked;

            leftContainer.PackStart(refreshButton);

            contentContainer.PackStart(leftContainer, true, true);
        }

        async void OptimiseAllImageAssetsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await WorkEngine.ApplyAsync(new OptimiseImageAssetWorkUnit(this.ImageAssets.Values.ToList())
                {
                    OnImageOptimisationFinishedDelegate = () => this.GatherImageAssetsAsync().ConfigureAwait(false)
                });
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void RefreshClicked(object sender, EventArgs e)
        {
            try
            {
                GatherImageAssetsAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void DeleteImageAssetButton_Clicked(object sender, EventArgs e)
        {
            WorkEngine.ApplyAsync(new DeleteImageAssetWorkUnit()
            {
                ImageAsset = SelectedImageAsset,
                OnImagesDeleted = (files) => GatherImageAssetsAsync().ConfigureAwait(false),
            });
        }

        void ImagesListView_DragStarted(object sender, DragStartedEventArgs e)
        {
            var dragOperation = e.DragOperation;
            var imageAsset = SelectedImageAsset;

            DoImageDragDrop(dragOperation, imageAsset);
        }

        void DoImageDragDrop(DragOperation dragOperation, IImageAsset imageAsset)
        {
            if (dragOperation == null || imageAsset == null)
            {
                return;
            }

            try
            {
                var project = ActiveDocument.CompilationProject;
                var fileInfo = ActiveDocument.FileInfo;

                if (imageAsset != null)
                {
                    var images = imageAsset.GetAssetsFor(project);

                    var insertion = imageAsset.Name;
                    if (project.IsAndroidProject())
                    {
                        var type = "drawable";

                        try
                        {
                            var image = images.First();
                            type = image.ProjectFolders.Last().Split('-').First();
                        }
                        catch { }

                        var isResources = (fileInfo.Directory.Parent?.Name?.Equals("Resources", StringComparison.OrdinalIgnoreCase) ?? false);
                        if (fileInfo.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
                        {
                            insertion = "Resource." + StringExtensions.FirstCharToUpper(type) + "." + SelectedImageAsset.ImageNameWithoutExtension;
                        }
                        else if (fileInfo.Extension.Equals(".axml", StringComparison.OrdinalIgnoreCase)
                            || fileInfo.Name.Equals("AndroidManifest.xml", StringComparison.OrdinalIgnoreCase)
                            || (fileInfo.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) && isResources))
                        {
                            insertion = "@" + type.ToLower() + "/" + SelectedImageAsset.ImageNameWithoutExtension;
                        }
                    }
                    else if (fileInfo.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        insertion = $"\"{SelectedImageAsset.Name}\"";
                    }

                    var source = dragOperation.Data as Xwt.TransferDataSource;
                    source.AddValue(insertion);

                    Analytics.Track("Image Drag-Drop");
                }
            }
            catch
            {
            }
        }

        void ApplyImageSelection(IImageAsset imageAsset)
        {
            projectChooser.Items.Clear();

            if (imageAsset != null)
            {
                foreach (var p in imageAsset.Projects)
                {
                    projectChooser.Items.Add(p, p.Name);
                }

                deleteImageAssetButton.Label = "Delete " + imageAsset.Name;
                deleteImageAssetButton.Sensitive = true;
            }
            else
            {
                deleteImageAssetButton.Sensitive = false;
                deleteImageAssetButton.Label = "Delete Image Asset";
            }

            if (pendingProjectFileSelection != null
                && imageAsset != null
                && imageAsset.Projects.Any(p => p.Name == pendingProjectFileSelection.CompilationProject.Name))
            {
                var pendingProject = imageAsset.Projects.FirstOrDefault(p => p.Name == pendingProjectFileSelection.CompilationProject.Name);
                if (pendingProject != null)
                {
                    projectChooser.SelectedIndex = EnumerableHelper.IndexOf(imageAsset.Projects, pendingProject);
                }
                else
                {
                    projectChooser.SelectedIndex = 0;
                }
            }
            else
            {
                projectChooser.SelectedIndex = 0;
            }
        }

        void SetProjectSelection(Project project)
        {
            this.selectedImageAssetsListDataStore.Clear();

            if (project == null || SelectedImageAsset == null)
            {
                return;
            }

            var files = SelectedImageAsset.GetAssetsFor(project);

            if (files == null)
            {
                return;
            }

            var selectionIndex = 0;
            foreach (var file in files)
            {
                var row = selectedImageAssetsListDataStore.AddRow();

                var sizeSummary = "";
                var preview = placeholderImage;
                if (File.Exists(file.FilePath))
                {
                    if (ImageHelper.IsImageFile(file.FilePath))
                    {
                        preview = Image.FromFile(file.FilePath).WithSize(PreviewSize, PreviewSize);
                        try
                        {
                            var size = ImageUtil.GetImageSize(file.FilePath);
                            sizeSummary = size.Width + "x" + size.Height;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        sizeSummary = "NA";
                    }

                    if (pendingProjectFileSelection != null
                        && file.FilePath == pendingProjectFileSelection.FilePath)
                    {
                        selectionIndex = row;
                        pendingProjectFileSelection = null;
                    }
                    else if (!string.IsNullOrEmpty(pendingImageAssetSelection))
                    {

                    }
                }

                selectedImageAssetsListDataStore.SetValue(row, selectedImageAssetsNameField, file.VirtualPath);
                selectedImageAssetsListDataStore.SetValue(row, selectedImageAssetsPreviewField, preview);
                selectedImageAssetsListDataStore.SetValue(row, selectedImageAssetsSizeSummaryField, sizeSummary);
            }

            selectedImageAssetsListView.SelectRow(selectionIndex);
        }

        void SetImageAssetSelection(IProjectFile projectFile)
        {
            SelectedProjectFile = projectFile;

            ApplyImageAssetSelectionUserInterface();
        }

        void ApplyImageAssetSelectionUserInterface()
        {
            if (SelectedProjectFile == null)
            {
                SelectedProjectFile = null;
                sizeLabel.Text = "Width: NA | Height: NA";
                imagePreview.Image = placeholderImage;
                fileSizeLabel.Text = "File Size: NA";
                fileSizeLabel.TooltipText = "";
                return;
            }

            var filePath = SelectedProjectFile.FilePath;

            if (ImageHelper.IsImageFile(filePath) && File.Exists(filePath))
            {
                try
                {
                    imagePreview.Image = Image.FromFile(filePath).WithBoxSize(250, 300);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }
            else
            {
                imagePreview.Image = Image.FromResource("mfractor_logo_grayscale.png").WithBoxSize(250, 220);
            }

            sizeLabel.Text = ImageUtil.GetImageSizeSummary(filePath);

            fileSizeLabel.Text = "File Size: " + FileSizeHelper.GetFormattedFileSize(filePath);
            fileSizeLabel.TooltipText = FileSizeHelper.GetRawFileSize(filePath);
        }

        void BuildRightPanel()
        {
            rightContainer = new VBox
            {
                WidthRequest = 320,
                Spacing = 1,
            };

            imagePreview = new ImageView
            {
                Image = Image.FromResource("mfractor_logo_grayscale.png").WithBoxSize(250, 220)
            };

            imageFrame = new FrameBox()
            {
                HeightRequest = 320,
                WidthRequest = 280,
                VerticalPlacement = WidgetPlacement.Center,
                HorizontalPlacement = WidgetPlacement.Center,
                Content = imagePreview,
                BorderColor = new Color(.2, .2, .2),
                BorderWidth = 1,
            };

            rightContainer.PackStart(imageFrame, true, true);

            fileSizeLabel = new Label()
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                TextAlignment = Alignment.Center,
                Text = "File Size: NA",
            };

            sizeLabel = new Label()
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                TextAlignment = Alignment.Center,
                Text = "Width: NA | Height: NA",
            };

            rightContainer.PackStart(fileSizeLabel);
            rightContainer.PackStart(sizeLabel);

            rightContainer.PackStart(new HSeparator());

            projectChooser = new ComboBox();
            projectChooser.SelectionChanged += ProjectChooser_SelectionChanged;
            rightContainer.PackStart(projectChooser);

            BuildImageAssetsListView();

            contentContainer.PackStart(rightContainer, true, true);
        }

        void ProjectChooser_SelectionChanged(object sender, EventArgs e)
        {
            var project = projectChooser.SelectedItem as Project;
            SetProjectSelection(project);
        }

        void BuildImageAssetsListView()
        {
            selectedImageAssetsListView = new ListBox();
            selectedImageAssetsListDataStore = new ListStore(selectedImageAssetsPreviewField, selectedImageAssetsSizeSummaryField, selectedImageAssetsNameField);

            var nameTextCell = new TextCellView(selectedImageAssetsNameField)
            {
                Editable = false
            };

            var sizeTextCell = new TextCellView(selectedImageAssetsSizeSummaryField)
            {
                Editable = false
            };

            selectedImageAssetsListView.Views.Add(new ImageCellView(selectedImageAssetsPreviewField));
            selectedImageAssetsListView.Views.Add(sizeTextCell);
            selectedImageAssetsListView.Views.Add(nameTextCell);

            selectedImageAssetsListView.DataSource = selectedImageAssetsListDataStore;
            selectedImageAssetsListView.SelectionChanged += SelectedImageAssetsListView_SelectionChanged;

            selectedImageAssetsListView.DragStarted += ImagesListView_DragStarted;
            selectedImageAssetsListView.SetDragDropTarget(TransferDataType.Text);
            selectedImageAssetsListView.SetDragSource(TransferDataType.Text);

            selectedImageAssetsListView.ButtonReleased += (sender, e) =>
            {
                if (e.Button == PointerButton.Right)
                {
                    var context = new ImageManagerCommandContext(null, SelectedProjectFile, Options, this);

                    ShowContextMenu(context, selectedImageAssetsListView, e.X, e.Position.Y);
                }
            };

            selectedImageAssetsListView.ButtonPressed += (sender, e) =>
            {
                if (e.Button == PointerButton.Left && e.MultiplePress == 2)
                {
                    Process.Start(SelectedProjectFile.FilePath);
                }
            };

            rightContainer.PackStart(selectedImageAssetsListView, true, true);
        }

        bool ShowContextMenu(IImageManagerCommandContext context, Xwt.Widget parent, double x, double y)
        {
            try
            {
                var commands = ImageManagerCommands.GetAvailableCommands(context);

                var menu = new ContextMenuDescription();

                foreach (var command in commands)
                {
                    var state = command.GetExecutionState(context);

                    menu.AddAction(state.Label, () =>
                    {
                        try
                        {
                            command.Execute(context);
                        }
                        catch (Exception ex)
                        {
                            log?.Exception(ex);
                        }
                    });
                }

                if (menu.Elements == null || !menu.Elements.Any())
                {
                    return false;
                }

                ContextMenuService.Show(parent, menu, (int)x, (int)y);
            }
            catch (Exception ex)
            {
                log?.Warning("Error while context menu popup. " + ex.Message);
            }
            return true;
        }

        void SelectedImageAssetsListView_SelectionChanged(object sender, EventArgs e)
        {
            var selection = selectedImageAssetsListView.SelectedRow;

            var image = SelectedImageAsset;
            if (image == null)
            {
                return;
            }

            if (!(projectChooser.SelectedItem is Project project))
            {
                return;
            }

            var files = image.GetAssetsFor(project);
            if (files == null || selection < 0 || selection >= files.Count)
            {
                return;
            }

            SetImageAssetSelection(files[selection]);
        }

        public void OnDrag(ICollectionItem item, DragOperation dragOperation)
        {
            if (item is ImageAssetCollectionItem imageItem)
            {
                DoImageDragDrop(dragOperation, imageItem.ImageAsset);
            }
        }

        public IImageAsset SelectedImageAsset => (imagesCollectionView.SelectedItem as ImageAssetCollectionItem)?.ImageAsset;

        public IProjectFile SelectedProjectFile { get; private set; }
    }
}
