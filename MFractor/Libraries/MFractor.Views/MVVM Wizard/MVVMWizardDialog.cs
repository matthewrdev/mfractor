using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Configuration;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeGeneration.Mvvm;
using MFractor.Maui.Mvvm;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using MFractor.Views.Branding;
using MFractor.Views.MVVMWizard.Settings;
using Xwt;
using Xwt.Drawing;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Workspace.Utilities;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Views.MVVMWizard
{
    public class MVVMWizardDialog : Dialog, IAnalyticsFeature
    {
        const string nameArgument = "$name$";

        readonly Logging.ILogger log = Logging.Logger.Create();

        public event EventHandler<MVVMWizardResultEventArgs> ViewViewModelCreated;

        [Import]
        IWorkEngine WorkEngine { get; set; }

        [Import]
        IAnalyticsService AnalyticsService { get; set; }

        [Import]
        IWorkspaceService WorkspaceService { get; set; }

        [Import]
        IProjectService ProjectService { get; set; }

        [Import]
        ILicensingService LicensingService { get; set; }

        [Import]
        IProjectMvvmSettingsService ProjectMvvmOptionsService { get; set; }

        [Import]
        ITextEditorFactory TextEditorFactory { get; set; }

        [Import]
        INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [Import]
        IViewViewModelGenerator ViewViewModelGenerator { get; set; }

        [Import]
        IXamlPlatformRepository XamlPlatforms { get; set; }

        [Import]
        IDispatcher Dispatcher { get; set; }

        IWorkUnit createViewWorkUnit;
        IWorkUnit createCodeBehindWorkUnit;
        IWorkUnit createViewModelWorkUnit;

        public ProjectIdentifier ProjectIdentifier { get; private set; }
        public ConfigurationId ConfigurationId { get; private set; }

        public bool OpenFilesOnCreation { get; set; } = true;

        public string AnalyticsEvent => "MVVM Wizard";

        public IXamlPlatform Platform { get; }

        readonly Image validationIcon = Image.FromResource("exclamation.png").WithSize(4.5, 15.5);
        readonly IReadOnlyList<ProjectIdentifier> projects;

        VBox root;
        VBox content;

        HBox topPanel;

        ComboBox projectSelectorCombo;

        HBox nameContainer;
        Label nameLabel;
        TextEntry nameEntry;
        ImageView nameValidationIcon;

        Button editSettingsButton;

        HBox codePreviewPanel;

        ComboBox filePicker;

        Label viewFilePathLabel;
        Label viewProjectLabel;
        ITextEditor viewPreviewEditor;

        Label viewModelFilePathLabel;
        Label viewModelProjectLabel;
        ITextEditor viewModelEditor;

        Button generateButton;

        ProjectMvvmSettings mvvmSettings;
        bool canProcessReturnKey = false;

        public MVVMWizardDialog(IReadOnlyList<ProjectIdentifier> projects, IXamlPlatform platform, ProjectIdentifier projectIdentifier = null)
        {
            if (projects == null)
            {
                throw new ArgumentNullException(nameof(projects));
            }

            if (!projects.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(projects) + " must have one or more available projects");
            }

            Resolver.ComposeParts(this);

            NamespaceDeclarationGenerator.ApplyConfiguration(ConfigurationId.Empty);
            ViewViewModelGenerator.ApplyConfiguration(ConfigurationId.Empty);

            Title = "View/ViewModel Wizard";
            Icon = Image.FromResource("mfractor_logo.png");

            Width = 1080;
            Height = 720;
            Platform = platform;

            this.projects = projects.ToList();

            Content = Build();

            SetTargetProject(projectIdentifier, platform);

            var options = GetOptions();

            ApplyAsync(options).ConfigureAwait(false);

            Task.Run(async () =>
            {
                await Task.Delay(1000);

                canProcessReturnKey = true;
            });
        }

        bool IsSupported(Microsoft.CodeAnalysis.Project project)
        {
            return XamlPlatforms.CanResolvePlatform(project);
        }

        void SetTargetProject(ProjectIdentifier projectIdentifier, IXamlPlatform platform)
        {
            if (projectIdentifier == null || !projects.Any(p => p.Guid == projectIdentifier.Guid))
            {
                var @default = projects.First();
                projectIdentifier = projects.FirstOrDefault(pId =>
                {
                    var p = ProjectService.GetProject(pId);

                    return IsSupported(p) && !p.IsMobileProject();
                }) ?? @default;
            }

            var project = projects.FirstOrDefault(pr => pr.Guid == projectIdentifier.Guid);
            projectSelectorCombo.SelectedIndex = EnumerableHelper.IndexOf(projects, project);

            ProjectIdentifier = projectIdentifier;
            ConfigurationId = ConfigurationId.Create(projectIdentifier);

            NamespaceDeclarationGenerator.ApplyConfiguration(ConfigurationId);
            ViewViewModelGenerator.ApplyConfiguration(ConfigurationId);

            nameEntry.Text = ViewViewModelGenerator.DefaultBaseName;

            LoadOptions(ProjectIdentifier, platform);
        }

        void LoadOptions(ProjectIdentifier projectIdentifier, IXamlPlatform platform)
        {
            mvvmSettings = ProjectMvvmOptionsService.Load(projectIdentifier, platform);
        }

        Task ApplyAsync(ViewViewModelGenerationOptions options)
        {
#pragma warning disable RECS0002 // Convert anonymous method to method group
            return Task.Run(async () =>
           {
               await Task.Delay(2);
               ApplyWorkUnits(options);
           });
#pragma warning restore RECS0002 // Convert anonymous method to method group
        }

        void ApplyWorkUnits(ViewViewModelGenerationOptions options)
        {
            try
            {
                createCodeBehindWorkUnit = null;
                createViewWorkUnit = null;
                createViewModelWorkUnit = null;

                var workUnits = ViewViewModelGenerator.Generate(ProjectIdentifier, WorkspaceService.CurrentWorkspace, Platform, options);

                createViewWorkUnit = workUnits.OfType<CreateProjectFileWorkUnit>().FirstOrDefault(r => r.FilePath.EndsWith(".xaml", StringComparison.Ordinal));
                createCodeBehindWorkUnit = workUnits.OfType<CreateProjectFileWorkUnit>().FirstOrDefault(r => r.FilePath.EndsWith(".xaml.cs", StringComparison.Ordinal));
                createViewModelWorkUnit = workUnits.OfType<CreateProjectFileWorkUnit>().FirstOrDefault(r => r.Identifier == "ViewModel");

                ApplyFilePreviews();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            Dispatcher.InvokeOnMainThread(() =>
           {
               UnBindEvents();

               ApplyFilePreviews();

               Validate();

               BindEvents();
           });
        }

        void Validate()
        {
            nameValidationIcon.Visible = !CSharpNameHelper.IsValidCSharpName(nameEntry.Text);
        }

        void BindEvents()
        {
            UnBindEvents();

            nameEntry.Changed += NameEntry_Changed;
            nameEntry.KeyReleased += NameEntry_KeyReleased;

            filePicker.SelectionChanged += FilePicker_SelectionChanged;

            generateButton.Clicked += GenerateButton_Clicked;

            editSettingsButton.Clicked += ShowAdvancedOptions_Clicked;

            projectSelectorCombo.SelectionChanged += ProjectSelectionChanged;
        }

        void ProjectSelectionChanged(object sender, EventArgs e)
        {
            UnBindEvents();

            try
            {
                var selection = (projectSelectorCombo.SelectedItem as ProjectIdentifier);
                SetTargetProject(selection, Platform);
            }
            finally
            {
                BindEvents();

                var options = GetOptions();
                ApplyAsync(options).ConfigureAwait(false);
            }
        }

        void NameEntry_KeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && canProcessReturnKey)
            {
                Generate();
            }
        }

        void ShowAdvancedOptions_Clicked(object sender, EventArgs e)
        {
            var settingsDialog = new ProjectMvvmSettingsDialog();
            settingsDialog.Load(ProjectIdentifier, Platform);
            settingsDialog.MvvmOptionsSaved += (s, args) =>
            {
                this.mvvmSettings = args.Settings;

                var options = GetOptions();
                ApplyAsync(options).ConfigureAwait(false);
            };
            settingsDialog.TransientFor = this;
            settingsDialog.Run(this);
        }

        void UnBindEvents()
        {
            nameEntry.Changed -= NameEntry_Changed;
            nameEntry.KeyReleased -= NameEntry_KeyReleased;

            filePicker.SelectionChanged -= FilePicker_SelectionChanged;

            generateButton.Clicked -= GenerateButton_Clicked;

            editSettingsButton.Clicked -= ShowAdvancedOptions_Clicked;

            projectSelectorCombo.SelectionChanged -= ProjectSelectionChanged;
        }

        void GenerateButton_Clicked(object sender, EventArgs e)
        {
            Generate();
        }

        void Generate()
        {
            if (!LicensingService.IsPaid)
            {
                WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit("The MVVM wizard is a MFractor Professional only feature. Please upgrade or request a trial.", AnalyticsEvent));
                return;
            }

            var options = GetOptions();

            var workUnits = ViewViewModelGenerator.Generate(ProjectIdentifier, WorkspaceService.CurrentWorkspace, Platform, options);

            foreach (var workUnit in workUnits)
            {
                if (workUnit is CreateProjectFileWorkUnit createFileWorkUnit)
                {
                    createFileWorkUnit.ShouldOpen = OpenFilesOnCreation;
                }
            }

            WorkEngine.ApplyAsync(workUnits).ConfigureAwait(false);
            ViewViewModelCreated?.Invoke(this, new MVVMWizardResultEventArgs(options));
            AnalyticsService.Track(this);

            this.Close();
            this.Dispose();
        }

        void NameEntry_Changed(object sender, EventArgs e)
        {
            UnBindEvents();


            var options = GetOptions();
            ApplyAsync(options).ConfigureAwait(false);
        }

        ViewViewModelGenerationOptions GetOptions()
        {
            var xmlnsPrefix = mvvmSettings.ViewBaseClassXmlnsPrefix;

            var viewFolder = mvvmSettings.ViewFolder.Replace(nameArgument, nameEntry.Text);
            var viewModelFolder = mvvmSettings.ViewModelFolder.Replace(nameArgument, nameEntry.Text);

            var viewNamespace = NamespaceDeclarationGenerator.GetNamespaceFor(ProjectIdentifier, viewFolder);
            var viewModelNamespace = NamespaceDeclarationGenerator.GetNamespaceFor(ProjectIdentifier, viewModelFolder);

            var baseName = nameEntry.Text;

            var viewName = baseName + mvvmSettings.ViewSuffix;
            var viewModelName = baseName + mvvmSettings.ViewModelSuffix;

            var viewProjectId = ProjectService.GetProject(mvvmSettings.ViewProjectId)?.GetIdentifier();
            var viewModelProjectId = ProjectService.GetProject(mvvmSettings.ViewModelProjectId)?.GetIdentifier();

            var options = new ViewViewModelGenerationOptions(viewName,
                                                             viewFolder,
                                                             mvvmSettings.ViewBaseClass,
                                                             viewNamespace,
                                                             xmlnsPrefix,
                                                             viewProjectId,
                                                             viewModelName,
                                                             viewModelFolder,
                                                             mvvmSettings.ViewModelBaseClass,
                                                             viewModelNamespace,
                                                             viewModelProjectId,
                                                             mvvmSettings.BindingContextConnectorId);

            return options;
        }

        Widget Build()
        {
            root = new VBox();

            content = new VBox();

            BuildTopPanel();

            content.PackStart(new VSeparator());

            BuildCodePreviewPanel();

            root.PackStart(content, true, true);

            generateButton = new Button("Generate");

            root.PackStart(generateButton);

            root.PackStart(new BrandedFooter());

            return root;
        }

        void BuildCodePreviewPanel()
        {
            codePreviewPanel = new HBox()
            {
                Spacing = 4,
            };

            var viewContainer = new VBox();

            viewProjectLabel = new Label();
            viewProjectLabel.Font = Xwt.Drawing.Font.SystemFont.WithSize(14).WithWeight(Xwt.Drawing.FontWeight.Bold);
            viewContainer.PackStart(viewProjectLabel);

            viewFilePathLabel = new Label();
            viewFilePathLabel.Font = Xwt.Drawing.Font.SystemFont.WithSize(14).WithWeight(Xwt.Drawing.FontWeight.Bold);
            viewContainer.PackStart(viewFilePathLabel);

            filePicker = new ComboBox();
            filePicker.Items.Add("XAML View");
            filePicker.Items.Add("Code Behind");
            filePicker.SelectedIndex = 0;

            viewContainer.PackStart(filePicker);

            viewContainer.PackStart(new HSeparator());

            viewPreviewEditor = TextEditorFactory.Create();

            viewContainer.PackStart(viewPreviewEditor.Widget, true, true);

            codePreviewPanel.PackStart(viewContainer, true, true);
            codePreviewPanel.PackStart(new VSeparator());

            var viewModelContainer = new VBox();

            viewModelProjectLabel = new Label();
            viewModelProjectLabel.Font = Xwt.Drawing.Font.SystemFont.WithSize(14).WithWeight(Xwt.Drawing.FontWeight.Bold);
            viewModelContainer.PackStart(viewModelProjectLabel);

            viewModelFilePathLabel = new Label();
            viewModelFilePathLabel.Font = Xwt.Drawing.Font.SystemFont.WithSize(14).WithWeight(Xwt.Drawing.FontWeight.Bold);
            viewModelContainer.PackStart(viewModelFilePathLabel);

            viewModelContainer.PackStart(new HSeparator());

            viewModelEditor = TextEditorFactory.Create();

            viewModelContainer.PackStart(viewModelEditor.Widget, true, true);

            codePreviewPanel.PackStart(viewModelContainer, true, true);

            content.PackStart(codePreviewPanel, true, true);
        }

        void FilePicker_SelectionChanged(object sender, EventArgs e)
        {
            ApplyFilePreviews();
        }

        void ApplyFilePreviews()
        {
            Dispatcher.InvokeOnMainThread(() =>
           {
               switch (filePicker.SelectedItem as string)
               {
                   case "XAML View":
                       ApplyWorkUnit(createViewWorkUnit, "application/xaml", "XAML View");
                       break;
                   case "Code Behind":
                       ApplyWorkUnit(createCodeBehindWorkUnit, "text/x-csharp", "Code Behind");
                       break;
                   default:
                       break;
               }

               if (createViewModelWorkUnit is CreateProjectFileWorkUnit createFileWorkUnit)
               {
                   viewModelFilePathLabel.Text = "File Path: " + createFileWorkUnit.FilePath;
                   viewModelProjectLabel.Text = "Project: " + createFileWorkUnit.TargetProject.Name;
                   viewModelEditor.Text = createFileWorkUnit.FileContent;
                   viewModelEditor.MimeType = "text/x-csharp";
               }
           });
        }

        void ApplyWorkUnit(IWorkUnit workUnit, string mimeType, string previewType)
        {
            var createFileWorkUnit = workUnit as CreateProjectFileWorkUnit;

            if (createFileWorkUnit != null)
            {
                viewFilePathLabel.Text = "File Path: " + createFileWorkUnit.FilePath;
                viewProjectLabel.Text = "Project: " + createFileWorkUnit.TargetProject.Name;
                viewPreviewEditor.Text = createFileWorkUnit.FileContent;
                viewPreviewEditor.MimeType = mimeType;
            }
            else
            {
                viewPreviewEditor.Text = $"The {previewType} preview is currently unavailable";
                viewPreviewEditor.Text = string.Empty;
            }
        }

        void BuildTopPanel()
        {
            var projectContainer = new HBox();

            projectContainer.PackStart(new Label("Target Project")
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Bold)
            });

            projectSelectorCombo = new ComboBox()
            {
                TooltipText = "What project should the new View/ViewModel pair be generated for?",
            };

            projectSelectorCombo.Items.Clear();
            foreach (var p in projects)
            {
                projectSelectorCombo.Items.Add(p, p.Name);
            }

            projectContainer.PackStart(projectSelectorCombo, true, true);

            content.PackStart(projectContainer);

            topPanel = new HBox();

            BuildNameAndFolderInputs();

            editSettingsButton = new Button("Settings")
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Bold),
            };
            topPanel.PackStart(editSettingsButton);
            topPanel.PackStart(new HSeparator());

            content.PackStart(topPanel);
        }

        void BuildNameAndFolderInputs()
        {
            nameContainer = new HBox();
            nameLabel = new Label("Name:");
            nameLabel.Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Bold);
            nameEntry = new TextEntry();
            nameEntry.Changed += (sender, e) =>
            {
                nameValidationIcon.Visible = string.IsNullOrEmpty(nameEntry.Text);
            };
            nameEntry.TooltipText = nameEntry.PlaceholderText = "What is the name of the new View/ViewModel pair?";
            nameEntry.Font = Font.SystemFont.WithSize(14);

            nameValidationIcon = new ImageView(this.validationIcon);
            nameValidationIcon.TooltipText = "Please enter a valid C# name for the new View/ViewModel pair.";
            nameValidationIcon.Visible = string.IsNullOrEmpty(nameEntry.Text);

            nameContainer.PackStart(nameLabel);
            nameContainer.PackStart(nameEntry, true, true);
            nameContainer.PackStart(nameValidationIcon);

            topPanel.PackStart(nameContainer, true, true);
        }
    }
}
