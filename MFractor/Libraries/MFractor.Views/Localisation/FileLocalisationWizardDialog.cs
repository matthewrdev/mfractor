using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.CodeSnippets;
using MFractor.Configuration;
using MFractor.Code.Documents;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Localisation;
using MFractor.Localisation.Importing;
using MFractor.Localisation.LocaliserRefactorings;
using MFractor.Localisation.WorkUnits;
using MFractor.Localisation.StringsProviders;
using MFractor.Logging;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using MFractor.Views.Branding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xwt;
using MFractor.Localisation.ValueProviders;
using MFractor.Work;

using MFractor.Localisation.CodeGeneration;
using MFractor.Ide;
using MFractor.Workspace.Utilities;

namespace MFractor.Views.Localisation
{
    public class FileLocalisationWizardDialog : Xwt.Dialog, IAnalyticsFeature
    {
        readonly ILogger log = Logger.Create();
        readonly object semanticModel;

        public class KeyMatch
        {
            public KeyMatch(string key)
            {
                Key = key;
            }

            public string Key { get; }

            public void Add(ILocalisationFile file, ILocalisationValue value)
            {
                if (!fileValues.ContainsKey(file))
                {
                    fileValues[file] = new List<ILocalisationValue>();
                }

                fileValues[file].Add(value);
            }

            readonly Dictionary<ILocalisationFile, List<ILocalisationValue>> fileValues = new Dictionary<ILocalisationFile, List<ILocalisationValue>>();

            public IReadOnlyList<ILocalisationFile> Files => fileValues.Keys.ToList();

            public string FileDescription
            {
                get
                {
                    var files = Files;
                    if (!files.Any())
                    {
                        return "no files";
                    }

                    if (files.Count > 1)
                    {
                        return files.Count + " files";
                    }

                    return files.FirstOrDefault().DisplayPath;
                }
            }

            public IEnumerable<ILocalisationValue> GetLocalisationValues(ILocalisationFile file)
            {
                if (!fileValues.ContainsKey(file))
                {
                    return Enumerable.Empty<ILocalisationValue>();
                }

                return fileValues[file];
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }

        Xwt.VBox root;
        Xwt.ComboBox projectsComboBox;

        ListView listView;
        ListStore listDataStore;
        DataField<bool> includeFileField;
        DataField<string> fileNameField;

        HBox resourceKeyContainer;
        ImageView resourceKeyErrorIcon;
        Xwt.TextEntry resourceKeyEntry;
        Label matchingResourceKeysLabel;
        Xwt.ComboBox matchingResourceKeysComboBox;

        HBox resourceValueContainer;
        ImageView resourceValueValidationIcon;
        Xwt.TextEntry resourceValueEntry;

        Xwt.TextEntry localisationExpressionSnippetEntry;
        Xwt.TextEntry localisationExpressionPreview;

        Button fixResourceKeyButton;

        Xwt.TextEntry resourceCommentEntry;

        HBox shouldCreateLocalisationFileContainer;

        public ILocalisableString Target { get; private set; }

        public Project Project => projectsComboBox.SelectedItem as Project;
        public IReadOnlyList<Project> Projects { get; }

        public ILocalisableStringsProvider LocalisableStringsProvider { get; private set; }
        public ILocalisationValuesProvider LocalisationValuesProvider { get; private set; }
        public ILocaliserRefactoring LocaliserRefactoring { get; private set; }

        public IReadOnlyList<ILocalisableString> Targets { get; private set; }
        public IReadOnlyDictionary<ILocalisationFile, IReadOnlyList<ILocalisationValue>> LocalisationResources { get; private set; }
        public IReadOnlyList<ILocalisationFile> ResourceFiles { get; private set; }

        public Command PreviousCommand { get; private set; }
        public Command ReplaceCommand { get; private set; }
        public Command NextCommand { get; private set; }

        public string AnalyticsEvent => "Localisation Wizard";

        public const string LastWindowLocationXKey = "com.mfractor.localisation.last_window_location_x";
        public const string LastWindowLocationYKey = "com.mfractor.localisation.last_window_location_y";

        public const string LocalisationChoiceBaseKey = "com.mfractor.localisation.choice";
        public const string LocalisationExpressionSnippetBaseKey = "com.mfractor.localisation.expression_snippet";

        int totalLocalisationsForSession = 0;

        public string DefaultFile { get; }
        public IParsedDocument Document { get; }

        [Import]
        IWorkEngine WorkEngine { get; set; }

        [Import]
        IUserOptions UserOptions { get; set; }

        [Import]
        IActiveDocument ActiveDocument { get; set; }

        [Import]
        IAnalyticsService AnalyticsService { get; set; }

        [Import]
        ILocalisationService LocalisationService { get; set; }

        [Import]
        ILocalisationValuesProviderRepository LocalisationValuesProviderRepository { get; set; }

        [Import]
        ILocalisableStringsProviderRepository LocalisableStringsProviderRepository { get; set; }

        [Import]
        ILocaliserRefactoringRepository LocaliserRefactoringRepository { get; set; }

        [Import]
        IResXFileGenerator ResXFileGenerator { get; set; }

        [Import]
        ILicensingService LicensingService { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }

        [Import]
        ICodeSnippetFactory CodeSnippetFactory { get; set; }

        [Import]
        IDispatcher Dispatcher { get; set; }

        public bool HasLastLocation
        {
            get
            {
                if (!UserOptions.HasKey(LastWindowLocationXKey)
                    || !UserOptions.HasKey(LastWindowLocationYKey))
                {
                    return false;
                }

                return true;
            }
        }

        public Point LastWindowLocation
        {
            get
            {
                if (!UserOptions.HasKey(LastWindowLocationXKey)
                    || !UserOptions.HasKey(LastWindowLocationYKey))
                {
                    return default;
                }

                var x = UserOptions.Get(LastWindowLocationXKey, this.Location.X);
                var y = UserOptions.Get(LastWindowLocationYKey, this.Location.Y);

                return new Point(x, y);
            }
        }

        protected override void OnHidden()
        {
            UserOptions.Set(LastWindowLocationXKey, this.Location.X);
            UserOptions.Set(LastWindowLocationYKey, this.Location.Y);

            var defaultCode = LocaliserRefactoring.LocalisationCodeSnippet.GetFormattedCode(EmptyCodeSnippetArgumentMode.Name);
            if (localisationExpressionSnippetEntry.Text != defaultCode)
            {
                var key = GetLocalisationExpressionSnippetKey(Project, this.Document);
                UserOptions.Set(key, localisationExpressionSnippetEntry.Text);
            }

            base.OnHidden();
        }

        public FileLocalisationWizardDialog(Project project,
                                            IReadOnlyList<Project> projects,
                                            TextSpan? targetSpan,
                                            IParsedDocument document,
                                            object semanticModel,
                                            string defaultFile)
        {
            Resolver.ComposeParts(this);

            Projects = projects;

            Document = document ?? throw new ArgumentNullException(nameof(document));
            this.semanticModel = semanticModel;
            DefaultFile = defaultFile;

            this.Title = "Localisation Wizard";
            this.Width = 300;

            LocalisableStringsProvider = LocalisableStringsProviderRepository.GetSupportedStringsProvider(project, Document.FilePath);

            Targets = LocalisableStringsProvider.RetrieveLocalisableStrings(Document, semanticModel).ToList();
            Target = Targets.FirstOrDefault();

            if (targetSpan != null)
            {
                Target = Targets.FirstOrDefault(t => t.Span.IntersectsWith(targetSpan.Value)) ?? Targets.FirstOrDefault();
            }

            Build(project);

            BuildCommands();

            BindEvents();

            SetTargetProject(project);

            ApplyLocalisationTemplate();

            Validate();

            FocusTarget(Target);

            if (HasLastLocation)
            {
                Location = LastWindowLocation;
            }
        }

        void SetTargetProject(Project project)
        {
            UnbindEvents();

            try
            {
                LocalisationValuesProvider = LocalisationValuesProviderRepository.GetSupportedValuesProvider(project);
                LocaliserRefactoring = LocaliserRefactoringRepository.GetSupportedLocaliserRefactoring(project, Document.FilePath);

                var configId = ConfigurationId.Create(project.GetIdentifier());

                LocaliserRefactoring.ApplyConfiguration(configId);

                ResourceFiles = LocalisationValuesProvider.RetrieveLocalisationFiles(project).ToList();
                LocalisationResources = LocalisationValuesProvider.ProvideLocalisationValues(ResourceFiles);

                shouldCreateLocalisationFileContainer.Visible = !ResourceFiles.Any();

                var defaultChoice = LocalisationValuesProvider.GetDefaultFile(ResourceFiles, DefaultFile);

                SetAvailableFiles(ResourceFiles);
            }
            finally
            {
                BindEvents();
            }
        }

        void ApplyLocalisationTemplate()
        {
            var snippet = LocaliserRefactoring.LocalisationCodeSnippet;

            var defaultCode = snippet.GetFormattedCode(EmptyCodeSnippetArgumentMode.Name);

            var key = GetLocalisationExpressionSnippetKey(Project, Document);

            localisationExpressionSnippetEntry.Text = UserOptions.Get(key, defaultCode);

            var operation = GetLocalisationOperation(true);

            CreateLocalisationExpressionPreview(operation);

            var description = snippet.Description;

            if (snippet.Arguments.Any())
            {
                description += "\n\nArguments:";

                foreach (var arg in snippet.Arguments)
                {
                    description += "\n * " + arg.Name + ": " + arg.Description;
                }
            }

            localisationExpressionSnippetEntry.TooltipText += description;
        }

        void SetAvailableFiles(IReadOnlyList<ILocalisationFile> resourceFiles)
        {
            this.listDataStore.Clear();

            if (resourceFiles != null)
            {
                for (var i = 0; i < resourceFiles.Count; ++i)
                {
                    var file = resourceFiles[i];
                    var row = listDataStore.AddRow();
                    listDataStore.SetValue(row, fileNameField, file.DisplayPath);

                    var key = GetLocalisationFileSelectedPreferencesKey(Project, file);

                    listDataStore.SetValue(row, includeFileField, UserOptions.Get(key, true));
                }
            }
        }

        string GetLocalisationFileSelectedPreferencesKey(Project project, ILocalisationFile file)
        {
            return LocalisationChoiceBaseKey + "." + project.Name.ToLower() + "." + CSharpNameHelper.ConvertToValidCSharpName(file.DisplayPath).ToLower();
        }

        string GetLocalisationExpressionSnippetKey(Project project, IParsedDocument document)
        {
            return LocalisationExpressionSnippetBaseKey + "." + project.Name.ToLower() + document.Extension;
        }

        void BindEvents()
        {
            UnbindEvents();

            projectsComboBox.SelectionChanged += ProjectsComboBox_SelectionChanged;
            fixResourceKeyButton.Clicked += FixResourceKeyButton_Clicked;
            resourceKeyEntry.Changed += ResourceKeyEntry_Changed;
            localisationExpressionSnippetEntry.Changed += LocalisationExpressionSnippetEntry_Changed;
        }

        void ProjectsComboBox_SelectionChanged(object sender, EventArgs e)
        {
            SetTargetProject(projectsComboBox.SelectedItem as Project);
        }

        void UnbindEvents()
        {
            projectsComboBox.SelectionChanged -= ProjectsComboBox_SelectionChanged;
            fixResourceKeyButton.Clicked -= FixResourceKeyButton_Clicked;
            resourceKeyEntry.Changed -= ResourceKeyEntry_Changed;
            localisationExpressionSnippetEntry.Changed -= LocalisationExpressionSnippetEntry_Changed;
        }

        void BuildCommands()
        {
            PreviousCommand = new Command("Previous", "Previous");
            ReplaceCommand = new Command("Replace", "Replace");
            NextCommand = new Command("Next", "Next");

            Buttons.Add(new Xwt.DialogButton(PreviousCommand));
            Buttons.Add(new Xwt.DialogButton(NextCommand));
            Buttons.Add(new Xwt.DialogButton(ReplaceCommand));
        }

        void Build(Project project)
        {
            root = new Xwt.VBox();

            root.PackStart(new Xwt.Label()
            {
                Markup = "<b>Projects:</b>"
            });

            projectsComboBox = new ComboBox();
            foreach (var p in Projects)
            {
                projectsComboBox.Items.Add(p, p.Name);
            }
            projectsComboBox.SelectedIndex = EnumerableHelper.FindIndex(Projects, p => p == project);

            root.PackStart(projectsComboBox);

            root.PackStart(new Xwt.Label()
            {
                Markup = "<b>Resource Files:</b>"
            });

            BuildListView();

            shouldCreateLocalisationFileContainer = new HBox();

            shouldCreateLocalisationFileContainer.PackStart(new ImageView
                {
                    Image = Xwt.Drawing.Image.FromResource("exclamation-yellow.png").WithSize(4.5, 15.5),
                    TooltipText = "A new localisation file will be created.",
                    WidthRequest = 15
                });
            shouldCreateLocalisationFileContainer.PackStart(new Xwt.Label()
            {
                Markup = "A new localisation file will be created."
            });

            root.PackStart(shouldCreateLocalisationFileContainer);

            root.PackStart(new Xwt.Label()
            {
                Markup = "<b>Value:</b>"
            });

            resourceValueContainer = new HBox();

            resourceValueValidationIcon = new ImageView
            {
                Image = Xwt.Drawing.Image.FromResource("exclamation.png").WithSize(4.5, 15.5),
                TooltipText = "A string value to localise is required.",
                WidthRequest = 15
            };
            resourceValueContainer.PackStart(resourceValueValidationIcon);

            resourceValueEntry = new Xwt.TextEntry();
            resourceValueEntry.Changed += (sender, e) =>
            {
                Validate();
            };

            resourceValueContainer.PackStart(resourceValueEntry, true, true);

            root.PackStart(resourceValueContainer);

            matchingResourceKeysLabel = new Label()
            {
                Markup = "<b>Existing Keys:</b>"
            };

            root.PackStart(matchingResourceKeysLabel);

            matchingResourceKeysComboBox = new ComboBox();
            matchingResourceKeysComboBox.SelectionChanged += MatchingResourceKeysComboBox_SelectionChanged;
            root.PackStart(matchingResourceKeysComboBox);

            root.PackStart(new Xwt.Label()
            {
                Markup = "<b>Key:</b>"
            });

            resourceKeyContainer = new HBox();

            resourceKeyErrorIcon = new ImageView();
            resourceKeyErrorIcon.Image = Xwt.Drawing.Image.FromResource("exclamation.png").WithSize(4.5, 15.5);
            resourceKeyErrorIcon.TooltipText = "Enter a valid key.";
            resourceKeyErrorIcon.WidthRequest = 15;
            resourceKeyContainer.PackStart(resourceKeyErrorIcon);

            resourceKeyEntry = new Xwt.TextEntry();

            resourceKeyEntry.TooltipText = "Return to generate the localisation.\nShift+Return to use the existing resource key for this value.";

            resourceKeyEntry.KeyReleased += (sender, e) =>
            {
                if (e.Key == Key.Return && Validate())
                {
                    this.OnCommandActivated(ReplaceCommand);
                }
            };

            resourceKeyContainer.PackStart(resourceKeyEntry, true, true);

            fixResourceKeyButton = new Button
            {
                Image = Xwt.Drawing.Image.FromResource("wand.png").WithSize(20, 20),
                VerticalPlacement = WidgetPlacement.Center,
                HorizontalPlacement = WidgetPlacement.Center,
                TooltipText = "Convert the image name to be a valid C# symbol name."
            };
            resourceKeyContainer.PackStart(fixResourceKeyButton, false, false);

            root.PackStart(resourceKeyContainer);

            root.PackStart(new Xwt.Label()
            {
                Markup = "<b>Description:</b>",
            });

            resourceCommentEntry = new Xwt.TextEntry()
            {
            };
            root.PackStart(resourceCommentEntry);


            root.PackStart(new HSeparator());

            localisationExpressionSnippetEntry = new TextEntry()
            {
                TooltipText = "The parameterised code template that is used as the localisation expression to replace the inline string."
            };
            localisationExpressionPreview = new TextEntry()
            {
                TooltipText = "The transformed code template that is used as the localisation expression to replace the inline string.",
                Sensitive = false,
            };

            root.PackStart(new Xwt.Label()
            {
                Markup = "<b>Replace with:</b>",
            });

            root.PackStart(localisationExpressionSnippetEntry);
            root.PackStart(localisationExpressionPreview);

            root.PackStart(new HSeparator());
            root.PackEnd(new BrandedFooter());

            Content = root;
        }

        void FixResourceKeyButton_Clicked(object sender, EventArgs e)
        {
            resourceKeyEntry.Text = CSharpNameHelper.ConvertToValidCSharpName(resourceKeyEntry.Text);
            ValidateInBackground();
        }

        void ResourceKeyEntry_Changed(object sender, EventArgs e)
        {
            ValidateInBackground();

            var operation = GetLocalisationOperation(true);

            CreateLocalisationExpressionPreview(operation);
        }

        void LocalisationExpressionSnippetEntry_Changed(object sender, EventArgs e)
        {
            var operation = GetLocalisationOperation(true);

            CreateLocalisationExpressionPreview(operation);
        }

        void CreateLocalisationExpressionPreview(LocalisationOperation localisationOperation)
        {
            var snippet = GetLocalisationExpressionSnippet();

            var preview = LocaliserRefactoring.CreateLocalisationExpression(snippet, localisationOperation, this.Project, this.Document);

            localisationExpressionPreview.Text = preview;
        }

        ICodeSnippet GetLocalisationExpressionSnippet()
        {
            return CodeSnippetFactory.CreateSnippet("Localisation Expression", "Localisation Expression", localisationExpressionSnippetEntry.Text);
        }

        void MatchingResourceKeysComboBox_SelectionChanged(object sender, EventArgs e)
        {
            var item = matchingResourceKeysComboBox.SelectedItem as KeyMatch;

            if (item != null)
            {
                resourceKeyEntry.Text = item.Key;
            }
        }

        void BuildListView()
        {
            this.listView = new ListView();
            listView.MinHeight = 100;

            includeFileField = new DataField<bool>();
            fileNameField = new DataField<string>();

            listDataStore = new ListStore(includeFileField, fileNameField);
            listView.DataSource = listDataStore;
            listView.GridLinesVisible = GridLines.Horizontal;

            var includeCell = new CheckBoxCellView { Editable = true, ActiveField = includeFileField };

            var fileNameCell = new TextCellView() { Editable = false, TextField = fileNameField };

            listView.Columns.Add("Include", includeCell);
            listView.Columns.Add(new ListViewColumn("File                  ", fileNameCell));

            listView.Columns[1].CanResize = true;

            root.PackStart(listView, true, true);


        }

        async Task<bool> LoadEntriesAsync()
        {
            var result = await Task.Run(() =>
            {
                LocalisationResources = LocalisationValuesProvider.ProvideLocalisationValues(ResourceFiles);

                return true;
            });

            ValidateInBackground();

            return result;
        }

        void FocusTarget(ILocalisableString target)
        {
            Target = target;

            if (target != null)
            {
                resourceValueEntry.Text = target.Value;
                resourceValueEntry.ReadOnly = true;
                resourceValueEntry.CanGetFocus = false;

                resourceKeyEntry.Text = "";
                resourceKeyEntry.SetFocus();

                ActiveDocument.SetCaretOffset(target.Span.Start);
                ActiveDocument.SetSelection(target.Span);
            }

            LoadMatchingKeySelections(target);

            var traits = new Dictionary<string, string>
            {
                ["Context"] = ActiveDocument.FileInfo.Extension,
                ["Localisations"] = totalLocalisationsForSession.ToString()
            };

            AnalyticsService.Track(this, traits);
        }

        void LoadMatchingKeySelections(ILocalisableString target)
        {
            matchingResourceKeysComboBox.Items.Clear();

            var matches = GetMatchingKeys(target);

            if (matches != null && matches.Any())
            {
                foreach (var match in matches)
                {
                    matchingResourceKeysComboBox.Items.Add(match.Value, match.Key + " in " + match.Value.FileDescription);
                }
                matchingResourceKeysComboBox.SelectedIndex = 0;
            }

            matchingResourceKeysComboBox.Visible = matchingResourceKeysLabel.Visible = matchingResourceKeysComboBox.Items.Any();
        }

        IReadOnlyDictionary<string, KeyMatch> GetMatchingKeys(ILocalisableString localisableString)
        {
            var matches = new Dictionary<string, KeyMatch>();

            if (localisableString != null)
            {
                foreach (var file in LocalisationResources)
                {
                    var match = file.Value.FirstOrDefault(value => value.Value == localisableString.Value);

                    if (match != null)
                    {
                        if (!matches.ContainsKey(match.Key))
                        {
                            matches.Add(match.Key, new KeyMatch(match.Key));
                        }

                        matches[match.Key].Add(file.Key, match);
                    }
                }
            }

            return matches;
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

        bool Validate()
        {
            var isValid = true;

            isValid &= ValidateResourceKey();
            isValid &= ValidateResourceValue();

            if (isValid)
            {
                EnableCommand(ReplaceCommand);
            }
            else
            {
                DisableCommand(ReplaceCommand);
            }

            return isValid;
        }

        bool ValidateResourceValue()
        {
            var isValid = !string.IsNullOrEmpty(resourceValueEntry.Text);

            resourceValueValidationIcon.Visible = !isValid;

            return isValid;
        }

        bool ValidateResourceKey()
        {
            var isValid = true;

            if (!CSharpNameHelper.IsValidCSharpName(resourceKeyEntry.Text))
            {
                resourceKeyErrorIcon.TooltipText = "Enter a valid key.";
                fixResourceKeyButton.Visible = !string.IsNullOrEmpty(resourceKeyEntry.Text);
                resourceKeyErrorIcon.Visible = true;
                isValid = false;
            }
            else
            {
                fixResourceKeyButton.Visible = false;
                resourceKeyErrorIcon.Visible = false;
            }

            return isValid;
        }

        IEnumerable<string> GetFileChoices(string key, string value)
        {
            var choices = new List<string>();

            for (var i = 0; i < ResourceFiles.Count; ++i)
            {
                var file = ResourceFiles[i];
                var isSelected = listDataStore.GetValue(i, includeFileField);

                var canAdd = true;

                if (LocalisationResources.ContainsKey(file))
                {
                    var targets = LocalisationResources[file];

                    if (targets.Any(t => t.Key == key && t.Value == value))
                    {
                        canAdd = false;
                    }
                }

                if (isSelected && canAdd)
                {
                    choices.Add(file.FullPath);
                }
            }

            return choices;
        }

        protected override async void OnCommandActivated(Command cmd)
        {
            if (cmd == ReplaceCommand)
            {
                await ReplaceAsync();
            }
            else if (cmd == NextCommand)
            {
                Next();
            }
            else if (cmd == PreviousCommand)
            {
                Previous();
            }
        }

        void Previous()
        {
            var old = Targets.FirstOrDefault(t => t.IsSame(Target));

            var index = 0;
            if (old != null)
            {
                index = EnumerableHelper.FindIndex(Targets, old);

                if (index - 1 < 0)
                {
                    index = Targets.Count - 1;
                }
                else if (index > 0)
                {
                    index--;
                }
            }

            FocusTarget(Targets[index]);
        }

        void Next()
        {
            var old = Targets.FirstOrDefault(t => t.IsSame(Target));

            var index = 0;
            if (old != null)
            {
                index = EnumerableHelper.FindIndex(Targets, old);

                if (index + 1 >= Targets.Count)
                {
                    index = 0;
                }
                else if (index >= 0)
                {
                    index++;
                }
            }

            FocusTarget(Targets[index]);
        }

        async Task ReplaceAsync()
        {
            if (!LicensingService.IsPaid)
            {
                await WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit($"The Localization Wizard is a Professional-only MFractor feature. Please upgrade or request a trial.", this.AnalyticsEvent));
                return;
            }

            var oldTargets = Targets;

            var result = this.GetLocalisationOperation(false);

            var workUnits = LocaliserRefactoring.CreateLocalisationValues(result, this.Project, this.Document).ToList();

            var expression = LocaliserRefactoring.CreateLocalisationExpression(GetLocalisationExpressionSnippet(), result, Project, Document);

            var replaceTextWorkUnit = new ReplaceTextWorkUnit()
            {
                FilePath = Document.FilePath,
                Span = Target.Span,
                Text = expression,
            };

            workUnits.Add(replaceTextWorkUnit);

            await WorkEngine.ApplyAsync(workUnits);

            await LoadEntriesAsync();

            var targets = oldTargets;

            var appliedTarget = Targets.FirstOrDefault(t => t.IsSame(Target));

            var index = 0;
            if (appliedTarget != null)
            {
                index = EnumerableHelper.FindIndex(oldTargets, appliedTarget);
            }

            Targets = RebuildTargets(appliedTarget, targets, workUnits);

            if (Targets == null || !Targets.Any())
            {
                DialogsService.ShowMessage("All strings in this document have been localised.", "Ok");
                Close();
                return;
            }

            if (index >= Targets.Count)
            {
                index = 0;
            }

            FocusTarget(Targets[index]);
            ValidateInBackground();
        }

        IReadOnlyList<ILocalisableString> RebuildTargets(ILocalisableString targetToRemove, IReadOnlyList<ILocalisableString> targets, IReadOnlyList<IWorkUnit> workUnits)
        {
            var result = new List<ILocalisableString>();

            var orderedWorkUnits = workUnits.OfType<ReplaceTextWorkUnit>().Where(f => f.FilePath == ActiveDocument.FilePath).OrderBy(f => f.Span.Start).ToList();

            if (orderedWorkUnits == null || !orderedWorkUnits.Any())
            {
                return targets;
            }

            foreach (var target in targets)
            {
                if (target.IsSame(targetToRemove))
                {
                    continue;
                }

                var indent = 0;
                foreach (var workUnit in orderedWorkUnits)
                {
                    if (target.Span.End < workUnit.Span.Start)
                    {
                        continue;
                    }

                    var intesection = workUnit.Span.Intersection(target.Span);

                    if (intesection.HasValue)
                    {
                        indent += intesection.Value.Length;
                    }
                    else
                    {
                        indent += workUnit.Text.Length - workUnit.Span.Length;
                    }
                }

                var span = new TextSpan(target.Span.Start + indent, target.Span.Length);
                result.Add(new LocalisableString(target.Value, target.FilePath, span));
            }

            return result;
        }

        LocalisationOperation GetLocalisationOperation(bool isPreview)
        {
            if (!isPreview)
            {
                totalLocalisationsForSession++;
            }

            return new LocalisationOperation(Target,
                                             Project,
                                             resourceKeyEntry.Text,
                                             resourceValueEntry.Text,
                                             resourceCommentEntry.Text,
                                             ResourceFiles.Select(lf => lf.FullPath),
                                             GetFileChoices(resourceKeyEntry.Text, resourceValueEntry.Text));
        }

        protected override void OnShown()
        {
            base.OnShown();

            resourceKeyEntry.SetFocus();
        }
    }
}
