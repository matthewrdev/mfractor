using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Utilities;
using MFractor.Views.Branding;
using MFractor.Views.Controls;
using MFractor.Views.Controls.Collection;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Xwt;

namespace MFractor.Views.GenerateCodeFiles
{
    public class GenerateCodeFilesDialog : Dialog
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        public IWorkEngine WorkEngine { get; set; }

        [Import]
        public ILicensingService LicensingService { get; set; }

        VBox root;

        HBox mainContainer;

        VBox leftPanel;

        Label description;
        Label nameLabel;
        TextEntry nameEntry;

        Label projectLabel;
        ComboBox projectPicker;
        CollectionView projectCollection;

        Label folderLabel;
        TextEntry folderEntry;

        Button generateButton;

        WorkUnitPreviewControl workUnitPreviewControl;

        readonly DateTime displayedTime = DateTime.UtcNow;
        readonly ProjectSelectorMode projectSelectorMode;
        BrandedFooter footer;

        public GenerateCodeFilesWorkUnit WorkUnit { get; private set; }

        public event EventHandler<GenerateCodeFilesEventArgs> GenerateInterfaceResultEvent;

        public GenerateCodeFilesDialog(ProjectSelectorMode projectSelectorMode)
        {
            Resolver.ComposeParts(this);

            Width = 960;
            Height = 640;

            this.projectSelectorMode = projectSelectorMode;

            Build();

            BindEvents();
        }

        void Build()
        {
            root = new VBox();
            mainContainer = new HBox();

            BuildLeftPanel();

            BuildRightPanel();

            root.PackStart(mainContainer, true, true);

            generateButton = new Button("Generate");
            generateButton.Clicked += GenerateButton_Clicked;

            root.PackStart(generateButton);

            footer = new BrandedFooter();

            root.PackStart(new HSeparator());
            root.PackStart(new BrandedFooter());

            Content = root;
        }

        public void SetWorkUnit(GenerateCodeFilesWorkUnit workUnit)
        {
            Title = workUnit.Title;

            WorkUnit = workUnit;
            description.Text = workUnit.Message;
            nameEntry.Text = workUnit.Name;
            nameEntry.Sensitive= workUnit.IsNameEditable;
            folderEntry.Text = WorkUnit.FolderPath;
            folderEntry.Sensitive = WorkUnit.IsFolderPathEditiable;
            requiresLicense = workUnit.RequiresLicense;
            footer.HelpUrl = workUnit.HelpUrl;

            ClearProjects();
            ApplyProjects(WorkUnit.Projects, workUnit.DefaultProject);

            CreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void ApplyProjects(IReadOnlyList<Project> projects, Project defaultProject)
        {
            UnbindEvents();

            try
            {
                if (projectSelectorMode == ProjectSelectorMode.Single)
                {
                    foreach (var project in projects)
                    {
                        projectPicker.Items.Add(project, project.Name);
                    }

                    if (defaultProject is null || !projects.Contains(defaultProject))
                    {
                        projectPicker.SelectedIndex = 0;
                    }
                    else
                    {
                        projectPicker.SelectedIndex = projects.IndexOf(defaultProject);
                    }
                }
                else
                {
                    var items = projects.Select(p => new ProjectSelectionCollectionItem(p)).ToList();
                    this.projectCollection.Items = items;
                }
            }
            finally
            {
                BindEvents();
            }
        }

        void ClearProjects()
        {
            projectPicker.Items.Clear();
            projectCollection.Clear();
        }

        void BindEvents()
        {
            UnbindEvents();

            nameEntry.KeyReleased += NameEntry_KeyReleased;
            folderEntry.KeyReleased += FolderEntry_KeyReleased;
            projectPicker.SelectionChanged += ProjectPicker_SelectionChanged;
            projectCollection.ItemsCheckSelectionChanged += ProjectSelection_ItemsCheckSelectionChanged;
        }

        void ProjectSelection_ItemsCheckSelectionChanged(object sender, EventArgs e)
        {
            CreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void CodeFilesSelectionComboBox_SelectionChanged(object sender, EventArgs e)
        {
            CreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void UnbindEvents()
        {
            nameEntry.KeyReleased -= NameEntry_KeyReleased;
            folderEntry.KeyReleased -= FolderEntry_KeyReleased;
            projectPicker.SelectionChanged -= ProjectPicker_SelectionChanged;
            projectCollection.ItemsCheckSelectionChanged -= ProjectSelection_ItemsCheckSelectionChanged;
        }

        void ProjectPicker_SelectionChanged(object sender, EventArgs e)
        {
            CreateOutputPreviewAsync().ConfigureAwait(false);
        }

        CancellationTokenSource cancellationTokenSource;
        bool requiresLicense;

        async Task ThrottledCreateOutputPreviewAsync()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            await Task.Delay(100);

            if (!cancellationToken.IsCancellationRequested)
            {
                await CreateOutputPreviewAsync();
            }
        }

        async Task CreateOutputPreviewAsync()
        {
            UnbindEvents();

            try
            {
                workUnitPreviewControl.IsWorking = true;
                var workUnits = Enumerable.Empty<IWorkUnit>();

                if (WorkUnit != null)
                {
                    var result = this.GetResult();
                    workUnits = await Task.Run(() => this.WorkUnit.GenerateCodeFilesDelegate(result));
                }

                workUnitPreviewControl.WorkUnits = workUnits.ToList();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
            finally
            {
                workUnitPreviewControl.IsWorking = false;
                BindEvents();
            }
        }

        void FolderEntry_KeyReleased(object sender, KeyEventArgs e)
        {
            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void BuildRightPanel()
        {
            workUnitPreviewControl = new WorkUnitPreviewControl();
            workUnitPreviewControl.WidthRequest = 600;

            mainContainer.PackStart(new HSeparator());

            mainContainer.PackStart(workUnitPreviewControl, true, true);
        }

        void BuildLeftPanel()
        {
            leftPanel = new VBox();

            description = new Label(WorkUnit?.Message ?? string.Empty);

            nameLabel = new Label()
            {
                Markup = "<b>Name:</b>"
            };

            nameEntry = new TextEntry()
            {
                PlaceholderText = "Enter a name for the new class",
            };

            projectLabel = new Label()
            {
                Markup = "<b>Project:</b>"
            };

            projectPicker = new ComboBox();

            var options = new CollectionViewOptions().WithSelectionCheckboxColumn("Include")
                                                     .WithPrimaryLabelColumn("Project");

            projectCollection = new CollectionView(options)
            {
                Title = "Projects",
                SearchPlaceholderText = "Search for project...",
                HeightRequest = 250,
            };

            folderLabel = new Label()
            {
                Markup = "<b>Folder:</b>"
            };

            folderEntry = new TextEntry()
            {
                PlaceholderText = "Enter the folder for the new file.",
                TooltipText = "The folder, relative to the projects root, that the new file will be placed into. If this folder path does not exist, it will be created.",
            };

            leftPanel.PackStart(description);

            leftPanel.PackStart(nameLabel);
            leftPanel.PackStart(nameEntry);

            leftPanel.PackStart(folderLabel);
            leftPanel.PackStart(folderEntry);

            if (projectSelectorMode == ProjectSelectorMode.Single)
            {
                leftPanel.PackStart(projectLabel);
                leftPanel.PackStart(projectPicker);
            }
            else
            {
                var children = projectCollection.Children.ToList();

                foreach (var child in children) // XWT vbox in vbox bug workaround.
                {
                    projectCollection.Remove(child);
                    leftPanel.PackStart(child, child.ExpandVertical, child.ExpandVertical);
                }
            }

            leftPanel.PackStart(new HSeparator());

            mainContainer.PackStart(leftPanel, true, WidgetPlacement.Fill);
        }
        
        void NameEntry_KeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return
                && DateTime.UtcNow - displayedTime > TimeSpan.FromSeconds(2)) // Don't allow instant commit.
            {
                Submit();
            }

            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void GenerateButton_Clicked(object sender, EventArgs e)
        {
            Submit();
        }

        public GenerateCodeFilesResult GetResult()
        {
            return new GenerateCodeFilesResult(nameEntry.Text, folderEntry.Text, GetSelectedProjects(), projectSelectorMode);
        }

        IReadOnlyList<Project> GetSelectedProjects()
        {
            if (projectSelectorMode == ProjectSelectorMode.Single)
            {
                return (projectPicker.SelectedItem as Project).AsList();
            }

            return projectCollection.Items.OfType<ProjectSelectionCollectionItem>()
                                         .Where(i => i.IsChecked)
                                         .Select(i => i.Project)
                                         .ToList();
        }

        void Submit()
        {
            if (requiresLicense && !LicensingService.IsPaid)
            {
                WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit($"{Title} is a Professional-only MFractor feature. Please upgrade or request a trial.", "Font Importer")).ConfigureAwait(false);
                return;
            }

            var result = GetResult();

            GenerateInterfaceResultEvent?.Invoke(this, new GenerateCodeFilesEventArgs(result));
            this.Close();
        }
    }
}
