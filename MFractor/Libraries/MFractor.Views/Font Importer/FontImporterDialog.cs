using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Configuration;
using MFractor.Fonts;
using MFractor.Fonts.WorkUnits;
using MFractor.Maui.CodeGeneration.Fonts;
using MFractor.Maui.CodeGeneration.Resources;
using MFractor.Maui.Configuration;
using MFractor.Maui.Utilities;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using MFractor.Views.Branding;
using Microsoft.CodeAnalysis;
using Xwt;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Views.FontImporter
{
    public class FontImporterDialog : Xwt.Dialog, IAnalyticsFeature
    {
        const string lastFolderSelection = "com.mfractor.font_importer.last_folder_selection";
        const string dialogTitle = "Import Font Asset";

        VBox root;
        HBox fontFileContainer;
        TextEntry fontFilePathEntry;
        Button selectFontFileButton;

        HBox fontAssetNameContainer;
        TextEntry fontAssetNameEntry;
        VBox bottomContainer;
        HBox fontFamilyKeyContainer;
        Label fontFamilyKeyLabel;
        TextEntry fontFamilyKeyEntry;
        ITextEditor xamlEditor;

        // TODO: Add a generate character code class check box.

        Button importButton;

        ListView projectsListView;
        ListStore projectsDataStore;

        ListView xamlFilesListView;
        ListStore xamlFilesDataStore;
        DataField<bool> includeXamlFileField;
        DataField<string> xamlFileNameField;

        readonly DataField<bool> projectSelectedField = new DataField<bool>();
        readonly DataField<string> projectNameField = new DataField<string>();
        readonly DataField<string> importedAssetPathField = new DataField<string>();

        IFont fontInformation;
        HBox xamlContainer;

        [Import]
        protected IConfigurationEngine ConfigurationEngine { get; set; }

        [Import]
        IAnalyticsService AnalyticsService { get; set; }

        [Import]
        IXamlPlatformRepository XamlPlatforms { get; set; }

        [Import]
        protected IWorkEngine WorkEngine { get; set; }

        [Import]
        protected IFontService FontService { get; set; }

        [Import]
        protected IFontImporter FontImporter { get; set; }

        [Import]
        protected IProjectService ProjectService { get; set; }

        [Import]
        protected ITextEditorFactory TextEditorFactory { get; set; }

        [Import]
        protected IDialogsService DialogsService { get; set; }

        [Import]
        protected IUserOptions UserOptions { get; set; }

        [Import]
        protected ILicensingService LicensingService { get; set; }

        [Import]
        protected IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        protected IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        IFontFamilyOnPlatformGenerator FontFamilyOnPlatformGenerator { get; }

        readonly List<IProjectFile> xamlFiles = new List<IProjectFile>();

        public Solution Solution { get; }
        public IReadOnlyList<Project> Projects { get; }
        public ProjectIdentifier ProjectIdentifier { get; }

        public string AnalyticsEvent => "Font Imported";

        public event EventHandler<FontImportResultEventArgs> FontImported;

        public FontImporterDialog(Solution solution, ProjectIdentifier projectIdentifier)
        {
            Resolver.ComposeParts(this);

            Solution = solution ?? throw new ArgumentNullException(nameof(solution));
            Projects = solution.GetMobileProjects() ?? new List<Project>();
            ProjectIdentifier = projectIdentifier;

            FontFamilyOnPlatformGenerator = ConfigurationEngine.Resolve<IFontFamilyOnPlatformGenerator>(ConfigurationId.Create(projectIdentifier));

            Build();

            PopulateAppXamlSelection();
        }

        void PopulateAppXamlSelection()
        {
            var projects = Solution.Projects;

            foreach (var project in projects)
            {
                if (!project.TryGetCompilation(out var compilation))
                {
                    continue;
                }

                var platform = XamlPlatforms.ResolvePlatform(project);
                if (platform == null)
                {
                    continue;
                }

                AppXamlConfiguration.ApplyConfiguration(ConfigurationId.Create(project.GetIdentifier()));


                var appXaml = AppXamlConfiguration.ResolveAppXamlFile(project.GetIdentifier(), platform);

                if (appXaml != null)
                {
                    xamlFiles.Add(appXaml);
                    AddFile(appXaml, true);
                }

                var resourceDictionaries = SymbolHelper.GetDeclaredDerivedTypes(compilation, platform.ResourceDictionary.MetaType);
                if (resourceDictionaries.Any())
                {
                    foreach (var rd in resourceDictionaries)
                    {
                        var syntax = rd.GetNonAutogeneratedSyntax();

                        if (syntax != null && syntax.SyntaxTree.FilePath.EndsWith(".xaml.cs", StringComparison.Ordinal))
                        {
                            var folderPath = Path.GetDirectoryName(syntax.SyntaxTree.FilePath);
                            var xamlFilePath = Path.GetFileNameWithoutExtension(syntax.SyntaxTree.FilePath);

                            xamlFilePath = Path.Combine(folderPath, xamlFilePath);

                            var projectFile = ProjectService.GetProjectFileWithFilePath(project, xamlFilePath);

                            if (projectFile != null)
                            {
                                xamlFiles.Add(projectFile);
                                AddFile(projectFile, false);
                            }
                        }
                    }
                }
            }
        }

        void AddFile(IProjectFile projectFile, bool isSelected)
        {
            var row = xamlFilesDataStore.AddRow();

            xamlFilesDataStore.SetValue(row, xamlFileNameField, projectFile.VirtualPath + " in " + projectFile.CompilationProject.Name);
            xamlFilesDataStore.SetValue(row, includeXamlFileField, isSelected);
        }

        void Build()
        {
            root = new VBox();
            Width = 960;
            Height = 720;
            Title = dialogTitle;

            BuildFontSelectionPanel();

            root.PackStart(new HSeparator());

            BuildProjectSelectionPanel();

            root.PackStart(new HSeparator());

            BuildFontFamilyCodePanel();

            importButton = new Button("Import Font")
            {
                TooltipText = "Import the font asset into the selected project, adding Info.plist entries into any iOS projects.",
            };

            root.PackStart(importButton);

            root.PackStart(new HSeparator());
            root.PackStart(new BrandedFooter("https://docs.mfractor.com/fonts/importing-fonts/", "Font Importer"));

            this.Content = root;

            BindEvents();
        }

        void UnbindEvents()
        {
            importButton.Clicked -= ImportButton_Clicked;
            selectFontFileButton.Clicked -= SelectFontFileButton_Clicked;
            fontAssetNameEntry.Changed -= FontAssetNameEntry_Changed;
            fontFamilyKeyEntry.Changed -= FontFamilyKeyEntry_Changed;
        }

        void BindEvents()
        {
            UnbindEvents();

            importButton.Clicked += ImportButton_Clicked;
            selectFontFileButton.Clicked += SelectFontFileButton_Clicked;
            fontAssetNameEntry.Changed += FontAssetNameEntry_Changed;
            fontFamilyKeyEntry.Changed += FontFamilyKeyEntry_Changed;
        }

        void FontFamilyKeyEntry_Changed(object sender, EventArgs e)
        {
            UnbindEvents();

            UpdateFontFamilyXAMLCode();

            BindEvents();
        }

        void FontAssetNameEntry_Changed(object sender, EventArgs e)
        {
            UnbindEvents();

            UpdateProjectResourcePathName();
            UpdateFontFamilyXAMLCode();

            BindEvents();
        }

        async void ImportButton_Clicked(object sender, EventArgs e)
        {
            if (!LicensingService.IsPaid)
            {
                await WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit($"The Font Importer is a Professional-only MFractor feature. Please upgrade or request a trial.", "Font Importer"));
                return;
            }

            var workUnits = new List<IWorkUnit>();

            var projects = GetSelectedProjects();
            var selectedXamlFiles = GetSelectedXamlFiles();

            foreach (var project in projects)
            {
                var importResult = FontImporter.ImportFont(project, fontFilePathEntry.Text, fontAssetNameEntry.Text);

                if (importResult != null && importResult.Any())
                {
                    workUnits.AddRange(importResult);
                }
            }

            foreach (var xamlFile in selectedXamlFiles)
            {
                var p = xamlFile.CompilationProject;

                FontFamilyOnPlatformGenerator.ApplyConfiguration(ConfigurationId.Create(p.GetIdentifier()));

                var platforms = new List<MFractor.PlatformFramework>()
                {
                    PlatformFramework.iOS,
                    PlatformFramework.Android,
                    PlatformFramework.UWP,
                };

                var node = FontFamilyOnPlatformGenerator.Generate(fontInformation, fontFamilyKeyEntry.Text, platforms);

                var insertions = InsertResourceEntryGenerator.Generate(xamlFile.CompilationProject, xamlFile.FilePath, node);

                if (insertions.Any())
                {
                    workUnits.AddRange(insertions);
                }
            }

            await WorkEngine.ApplyAsync(workUnits);

            var result = new FontImportResult(fontFilePathEntry.Text, fontAssetNameEntry.Text, projects, selectedXamlFiles, fontInformation, fontFamilyKeyEntry.Text);

            this.FontImported?.Invoke(this, new FontImportResultEventArgs(result));

            AnalyticsService.Track(this, new Dictionary<string, string>()
            {
                { "Font", fontInformation.FullName}
            });

            this.Close();
            this.Dispose();
        }

        async void SelectFontFileButton_Clicked(object sender, EventArgs e)
        {
            await SelectFontFile();
        }

        async Task<bool> SelectFontFile()
        {
            var folder = UserOptions.Get(lastFolderSelection, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            var fontFilter = new FileDialogFilter("Fonts", "*.ttf", "*.otf");

            var chooser = new Xwt.OpenFileDialog("Choose a font to import");
            chooser.Filters.Add(fontFilter);
            chooser.ActiveFilter = fontFilter;
            chooser.Multiselect = false;
            chooser.CurrentFolder = folder;

            var result = chooser.Run(this);

            var filePath = "";
            if (result)
            {
                filePath = chooser.FileName;
            }

            chooser.Dispose();

            if (!string.IsNullOrEmpty(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                UserOptions.Set(lastFolderSelection, fileInfo.Directory.FullName);
            }

            var fontResult = await SetFont(filePath);

            if (fontResult.Success == false)
            {
                if (!string.IsNullOrEmpty(fontResult.ErrorMessage))
                {
                    DialogsService.ShowError(fontResult.ErrorMessage);
                }
                return false;
            }

            return true;
        }

        class SetFontResult
        {
            public bool Success { get; }
            public string ErrorMessage { get; }

            public SetFontResult(bool success, string errorMessage)
            {
                Success = success;
                ErrorMessage = errorMessage;
            }
        }


        async Task<SetFontResult> SetFont(string filePath)
        {
            UnbindEvents();

            try
            {
                Reset();

                if (string.IsNullOrEmpty(filePath))
                {
                    return new SetFontResult(false, string.Empty);
                }

                if (!File.Exists(filePath))
                {
                    return new SetFontResult(false, filePath + " does not exist.");
                }

                var extension = Path.GetExtension(filePath);
                var isFont = extension.Equals(".otf", StringComparison.OrdinalIgnoreCase) || extension.Equals(".ttf", StringComparison.OrdinalIgnoreCase);

                if (!isFont)
                {
                    return new SetFontResult(false, filePath + " is not a valid font file. The font importer accepts .ttf or .otf font files only.");
                }

                fontInformation = FontService.GetFont(filePath);

                if (fontInformation == null)
                {
                    return new SetFontResult(false, "MFractor failed to read the needed information from the font file " + filePath + ".");
                }

                fontFilePathEntry.Text = filePath;
                fontAssetNameEntry.Text = Path.GetFileName(filePath);

                fontFamilyKeyEntry.Text = CSharpNameHelper.ConvertToValidCSharpName(fontInformation.FullName);

                UpdateProjectResourcePathName();

                UpdateFontFamilyXAMLCode();

                return new SetFontResult(true, string.Empty);
            }
            finally
            {
                BindEvents();
            }
        }

        void UpdateProjectResourcePathName()
        {
            for (var i = 0; i < projectsDataStore.RowCount; ++i)
            {
                if (string.IsNullOrEmpty(fontAssetNameEntry.Name))
                {
                    projectsDataStore.SetValue<string>(i, importedAssetPathField, "NA");
                }
                else
                {
                    var project = Projects[i];
                    if (project.IsAndroidProject())
                    {
                        projectsDataStore.SetValue<string>(i, importedAssetPathField, "Assets/" + fontAssetNameEntry.Text);
                    }
                    else if (project.IsAppleUnifiedProject())
                    {
                        projectsDataStore.SetValue<string>(i, importedAssetPathField, "Resources/" + fontAssetNameEntry.Text);
                    }
                    else if (project.IsUWPProject())
                    {
                        projectsDataStore.SetValue<string>(i, importedAssetPathField, "Assets/Fonts/" + fontAssetNameEntry.Text);
                    }
                }
            }
        }

        void Reset()
        {
            fontInformation = null;
            fontFilePathEntry.Text = string.Empty;
            fontAssetNameEntry.Text = string.Empty;

            UpdateProjectResourcePathName();

            UpdateFontFamilyXAMLCode();
        }

        void UpdateFontFamilyXAMLCode()
        {
            var projects = GetSelectedProjects();

            if (!projects.Any()
                || fontInformation == null
                || string.IsNullOrEmpty(fontFilePathEntry.Text)
                || string.IsNullOrEmpty(fontAssetNameEntry.Text))
            {
                xamlEditor.Text = string.Empty;
                return;
            }

            xamlEditor.Text = FontFamilyOnPlatformGenerator.GenerateXaml(fontAssetNameEntry.Text, fontInformation.Name, fontInformation.PostscriptName, fontInformation.Style, fontFamilyKeyEntry.Text, fontInformation.FamilyName, projects);
        }

        List<Project> GetSelectedProjects()
        {
            var selectedProjects = new List<Project>();

            if (Projects == null || !Projects.Any())
            {
                return selectedProjects;
            }

            for (var i = 0; i < projectsDataStore.RowCount; ++i)
            {
                if (projectsDataStore.GetValue<bool>(i, projectSelectedField))
                {
                    selectedProjects.Add(Projects[i]);
                }
            }

            return selectedProjects;
        }

        List<IProjectFile> GetSelectedXamlFiles()
        {
            var selectedXamlFiles = new List<IProjectFile>();

            if (xamlFiles == null || !xamlFiles.Any())
            {
                return selectedXamlFiles;
            }

            for (var i = 0; i < xamlFilesDataStore.RowCount; ++i)
            {
                if (xamlFilesDataStore.GetValue<bool>(i, includeXamlFileField))
                {
                    selectedXamlFiles.Add(xamlFiles[i]);
                }
            }

            return selectedXamlFiles;
        }

        void BuildFontSelectionPanel()
        {
            fontFileContainer = new HBox();

            fontFileContainer.PackStart(new Label("Font File Path: "));

            fontFilePathEntry = new TextEntry()
            {
                TooltipText = "The file path to the font file (otf or ttf)",
                PlaceholderText = "The file path to the font file (otf or ttf)",
                Sensitive = false,
            };


            fontFileContainer.PackStart(fontFilePathEntry, true, true);

            selectFontFileButton = new Button
            {
                Label = "Choose Font"
            };

            fontFileContainer.PackStart(selectFontFileButton);

            root.PackStart(fontFileContainer);

            fontAssetNameContainer = new HBox();

            fontAssetNameContainer.PackStart(new Label("Font Asset Name: "));

            fontAssetNameEntry = new TextEntry()
            {
                TooltipText = "The name of the imported font asset, excluding the Assets or Resources folder path.",
                PlaceholderText = "The name of the imported font asset, excluding the Assets or Resources folder path.",
            };

            fontAssetNameContainer.PackStart(fontAssetNameEntry, true, true);
            root.PackStart(fontAssetNameContainer);
        }

        void BuildProjectSelectionPanel()
        {
            root.PackStart(new Label("Projects")
            {
                Font = Xwt.Drawing.Font.SystemFont.WithSize(14).WithWeight(Xwt.Drawing.FontWeight.Bold),
            });

            projectsListView = new ListView();

            projectsDataStore = new ListStore(projectSelectedField, projectNameField, importedAssetPathField);
            projectsListView.DataSource = projectsDataStore;

            var checkbox = new CheckBoxCellView(projectSelectedField)
            {
                Editable = true,
            };

            checkbox.Toggled += (sender, e) =>
            {
                UpdateFontFamilyXAMLCode();
            };

            projectsListView.Columns.Add("Include Font?", checkbox);

            projectsListView.Columns.Add("Project Name", new TextCellView(projectNameField)
            {
                Editable = false,
            });

            projectsListView.Columns.Add("Result", new TextCellView(importedAssetPathField)
            {
                Editable = false,
            });

            projectsListView.WidthRequest = 500;
            projectsListView.HeightRequest = 200;

            foreach (var project in Projects)
            {
                var name = project.Name;
                var row = projectsDataStore.AddRow();

                projectsDataStore.SetValue(row, projectSelectedField, true);
                projectsDataStore.SetValue(row, projectNameField, project.Name);
                projectsDataStore.SetValue(row, importedAssetPathField, "NA");
            }

            root.PackStart(projectsListView);
        }

        void BuildFontFamilyCodePanel()
        {
            root.PackStart(new Label("FontFamily XAML")
            {
                Font = Xwt.Drawing.Font.SystemFont.WithSize(14).WithWeight(Xwt.Drawing.FontWeight.Bold),
                TooltipText = "MFractor can generate the XAML code needed for a assigning the FontFamily; this code can be pasted as a resource into your XAML file (such as the App.xaml) or included within a elements .FontFamily property.",
            });

            bottomContainer = new VBox();

            fontFamilyKeyContainer = new HBox();

            fontFamilyKeyLabel = new Label("Resource Key")
            {
                TooltipText = "What is the value of the x:Key to use for the FontFamily OnPlatform resource?",
            };

            fontFamilyKeyEntry = new TextEntry()
            {
                TooltipText = "What is the value of the x:Key to use for the FontFamily OnPlatform resource?",
                PlaceholderText = "What is the value of the x:Key to use for the FontFamily OnPlatform resource?",
            };

            fontFamilyKeyContainer.PackStart(fontFamilyKeyLabel);
            fontFamilyKeyContainer.PackStart(fontFamilyKeyEntry, true, true);

            bottomContainer.PackStart(fontFamilyKeyContainer);

            bottomContainer.PackStart(new HSeparator());

            xamlContainer = new HBox();

            BuildXamlSelectionListView();

            BuildXamlPreviewer();

            bottomContainer.PackStart(xamlContainer, true, true);

            root.PackStart(bottomContainer, true, true);
        }

        void BuildXamlPreviewer()
        {
            var previewCOntainer = new VBox();

            xamlEditor = TextEditorFactory.Create();
            xamlEditor.MimeType = "application/xaml";

            previewCOntainer.PackStart(new Label()
            {
                Markup = "<b>Font Declaration:</b>",
            });

            previewCOntainer.PackStart(xamlEditor.Widget, true, true);

            xamlContainer.PackStart(previewCOntainer, true, true);
        }

        void BuildXamlSelectionListView()
        {
            var listContainer = new VBox();

            listContainer.PackStart(new Label()
            {
                Markup = "<b>Add Font Declaration:</b>",
                TooltipText = "Select the App.xaml and ResourceDictionaries that you would like to add a declaration of this font to.",
            });

            xamlFilesListView = new ListView();

            includeXamlFileField = new DataField<bool>();
            xamlFileNameField = new DataField<string>();

            xamlFilesDataStore = new ListStore(includeXamlFileField, xamlFileNameField);
            xamlFilesListView.DataSource = xamlFilesDataStore;
            xamlFilesListView.GridLinesVisible = GridLines.Horizontal;

            var includeCell = new CheckBoxCellView { Editable = true, ActiveField = includeXamlFileField };

            var fileNameCell = new TextCellView() { Editable = false, TextField = xamlFileNameField };

            xamlFilesListView.Columns.Add("Include", includeCell);
            xamlFilesListView.Columns.Add(new ListViewColumn("File                  ", fileNameCell));

            xamlFilesListView.Columns[1].CanResize = true;

            listContainer.PackStart(xamlFilesListView, true, true);

            xamlContainer.PackStart(listContainer, true, true);
        }
    }
}
