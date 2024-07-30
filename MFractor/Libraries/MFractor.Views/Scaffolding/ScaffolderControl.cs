using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Configuration;

using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Code.Scaffolding;
using MFractor.Views.Branding;
using MFractor.Views.Controls;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Xwt;

namespace MFractor.Views.Scaffolding
{
    public class ScaffolderControl : VBox
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly HBox mainContainer;
        VBox leftPanel;

        Label description;
        TextEntry nameEntry;

        ListView suggestionsListView;
        ListStore suggestionsListStore;

        readonly DataField<string> suggestionsNameField = new DataField<string>();

        TextCellView suggestionsNameCell;
        readonly Button generateButton;

        public event EventHandler<ScaffoldingResultEventArgs> Confirmed;

        IScaffoldingContext context;

        WorkUnitPreviewControl workUnitPreviewControl;

        [Import]
        IScaffolderRepository ScaffolderRepository { get; set; }

        [Import]
        ILicensingService LicensingService { get; set; }

        [Import]
        IWorkEngine WorkEngine { get; set; }

        [Import]
        IUserOptions UserOptions { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }

        [Import]
        IDispatcher Dispatcher { get; set; }

        const string hasDisplayedWelcomeMessageKey = "com.mfractor.settings.scaffolder.has_display_welcome_message";

        public ScaffolderControl()
        {
            Resolver.ComposeParts(this);

            mainContainer = new HBox();

            BuildLeftPanel();

            BuildRightPanel();

            this.PackStart(mainContainer, true, true);

            generateButton = new Button("Generate");
            generateButton.Clicked += GenerateButton_Clicked;

            PackStart(generateButton);

            PackStart(new HSeparator());
            PackStart(new BrandedFooter("https://docs.mfractor.com/scaffolder/", "Scaffolder"));

            BindEvents();

            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);

            if (!UserOptions.Get(hasDisplayedWelcomeMessageKey, false))
            {
                UserOptions.Set(hasDisplayedWelcomeMessageKey, true);

                DialogsService.ShowMessage("The Scaffolder is an intelligent file creation wizard that can be used as an alternative to the File -> New experience.\n\nWhen you enter the name of your new file, the Scaffolder inspects the name for known patterns (EG: Interfaces begin with the letter 'I'), known file extensions (EG: .xaml or .resx) and the context that you are creating the file within.\n\nAs a result, you can simply type in a file name and extension and MFractor will infer your intended new file type and contents.\n\nThe Scaffolder is still an early feature, please submit feedback to matthew@mfractor.com.", "Ok");
            }
        }

        public IScaffolder SelectedScaffolder
        {
            get
            {
                if (SelectedSuggestion == null)
                {
                    return null;
                }

                var id = SelectedSuggestion.ScaffolderId;

                return ScaffolderRepository.GetScaffolder(id);
            }
        }

        public IScaffoldingSuggestion SelectedSuggestion
        {
            get
            {
                if (ScaffoldingSuggestions == null || !ScaffoldingSuggestions.Any())
                {
                    return null;
                }

                var row = this.suggestionsListView.SelectedRow;

                // Should never happen, sanity check.
                if (row >= ScaffoldingSuggestions.Count
                    || row < 0)
                {
                    return null;
                }

                return ScaffoldingSuggestions[row];
            }
        }

        public IScaffoldingInput Input => new ScaffoldingInput(nameEntry.Text);

        public IReadOnlyList<IScaffolder> AvailableScaffolders { get; private set; }
        public IReadOnlyList<IScaffoldingSuggestion> ScaffoldingSuggestions { get; private set; }

        public void SetScaffoldingContext(IScaffoldingContext scaffoldingContext, string folderPath)
        {
            context = scaffoldingContext;
            nameEntry.Text = folderPath ?? string.Empty;

            AvailableScaffolders = ScaffolderRepository.GetAvailableScaffolders(scaffoldingContext).ToList();

            try
            {
                UnbindEvents();
                var scaffolders = AvailableScaffolders.Where(s => s.CanSuggestInitialInput(context, Input)).ToList();

                if (scaffolders.Any())
                {
                    var suggestion = scaffolders.FirstOrDefault();

                    var input = suggestion.SuggestInitialInput(context, Input);

                    nameEntry.Text = input.RawInput;
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
            finally
            {
                BindEvents();
            }

            BuildScaffoldingSuggestions();
        }

        void BindEvents()
        {
            UnbindEvents();

            nameEntry.KeyReleased += NameEntry_KeyReleased;
            suggestionsListView.SelectionChanged += SuggestionsListView_SelectionChanged;
            suggestionsListView.KeyReleased += SuggestionsListView_KeyReleased;
        }

        void SuggestionsListView_SelectionChanged(object sender, EventArgs e)
        {
            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void UnbindEvents()
        {
            nameEntry.KeyReleased -= NameEntry_KeyReleased;
            suggestionsListView.SelectionChanged -= SuggestionsListView_SelectionChanged;
            suggestionsListView.KeyReleased -= SuggestionsListView_KeyReleased;
        }

        void ProjectPicker_SelectionChanged(object sender, EventArgs e)
        {
            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        CancellationTokenSource cancellationTokenSource;

        Task ThrottledCreateOutputPreviewAsync()
        {
            var input = Input;
            var suggestion = SelectedSuggestion;

            return Task.Run(async () =>
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
                    CreateOutputPreview(input, suggestion);
                }
            });

        }

        void CreateOutputPreview(IScaffoldingInput input, IScaffoldingSuggestion suggestion)
        {
            try
            {
                Dispatcher.InvokeOnMainThread(() =>
                {
                    UnbindEvents();
                    workUnitPreviewControl.IsWorking = true;
                });

                List<IWorkUnit> workUnits = null;

                if (suggestion != null)
                {
                    var scafolder = this.ScaffolderRepository.GetScaffolder(suggestion.ScaffolderId);

                    workUnits = scafolder.ProvideScaffolds(context, input, ScaffolderState.Empty, suggestion).ToList();
                }

                Dispatcher.InvokeOnMainThread(() =>
                {
                    workUnitPreviewControl.WorkUnits = workUnits;
                });
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
            finally
            {
                Dispatcher.InvokeOnMainThread(() =>
                {
                    workUnitPreviewControl.IsWorking = false;
                    BindEvents();
                });
            }
        }

        void FolderEntry_KeyReleased(object sender, KeyEventArgs e)
        {
            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void BuildRightPanel()
        {
            workUnitPreviewControl = new WorkUnitPreviewControl();

            mainContainer.PackStart(new HSeparator());

            mainContainer.PackStart(workUnitPreviewControl, true, true);
        }

        void BuildLeftPanel()
        {
            leftPanel = new VBox();

            description = new Label();

            nameEntry = new TextEntry()
            {
                PlaceholderText = "Start typing the name of your new file to begin scaffolding...",
            };

            leftPanel.PackStart(description);

            leftPanel.PackStart(nameEntry);

            leftPanel.PackStart(new HSeparator());

            BuildListView();

            mainContainer.PackStart(leftPanel);
        }

        void SuggestionsListView_KeyReleased(object sender, KeyEventArgs e)
        {
            var requiresRebuild = false;
            if (e.Key == Key.Up)
            {
                requiresRebuild = true;
                SelectPreviousSuggestion();
            }
            else if (e.Key == Key.Down)
            {
                requiresRebuild = true;
                SelectNextSuggestion();
            }
            else if (e.Key == Key.Return)
            {
                Submit();
                return;
            }

            if (requiresRebuild)
            {
                ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
            }
        }

        void NameEntry_KeyReleased(object sender, KeyEventArgs e)
        {
            UnbindEvents();

            try
            {
                if (e.Key == Key.Up)
                {
                    SelectPreviousSuggestion();
                }
                else if (e.Key == Key.Down)
                {
                    SelectNextSuggestion();
                }
                else if (e.Key == Key.Return)
                {
                    Submit();

                    return;
                }
                else
                {
                    BuildScaffoldingSuggestions();
                }
            }
            finally
            {
                BindEvents();
            }

            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void SelectNextSuggestion()
        {
            var row = suggestionsListView.SelectedRow;

            if (row >= suggestionsListStore.RowCount - 1)
            {
                return;
            }

            this.suggestionsListView.SelectRow(row++);
        }

        void SelectPreviousSuggestion()
        {
            var row = suggestionsListView.SelectedRow;

            if (row == 0)
            {
                return;
            }

            this.suggestionsListView.SelectRow(row--);
        }

        void BuildScaffoldingSuggestions()
        {
            var input = Input;

            var scaffolders = AvailableScaffolders.Where(c =>
            {
                try
                {
                    return c.CanProvideScaffolds(context, input, ScaffolderState.Empty);
                }
                catch (NotImplementedException)
                {
                    // Suppress
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }

                return false;
            });

            var previousSelection = suggestionsListView.SelectedRow;
            suggestionsListStore.Clear();

            var suggestions = new List<IScaffoldingSuggestion>();

            foreach (var scaffolder in scaffolders)
            {
                try
                {
                    var items = scaffolder.SuggestScaffolds(context, input, ScaffolderState.Empty);

                    if (items != null && items.Any())
                    {
                        suggestions.AddRange(items);
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            if (suggestions.Any())
            {
                ScaffoldingSuggestions = suggestions.OrderByDescending(s => s.Priority).ThenBy(s => s.Name).ToList();

                foreach (var suggestion in ScaffoldingSuggestions)
                {
                    var row = this.suggestionsListStore.AddRow();

                    try
                    {
                        suggestionsListStore.SetValue(row, suggestionsNameField, suggestion.Description);
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }
            }

            if (previousSelection >= 0 && previousSelection < suggestionsListStore.RowCount)
            {
                suggestionsListView.SelectRow(previousSelection);
            }
            else if (suggestions.Any())
            {
                suggestionsListView.SelectRow(0);
            }
        }

        void BuildListView()
        {
            suggestionsListView = new ListView();
            suggestionsListView.WidthRequest = 350;

            suggestionsListStore = new ListStore(suggestionsNameField);

            suggestionsNameCell = new TextCellView(suggestionsNameField)
            {
                Editable = false,
            };

            suggestionsListView.Columns.Add("Name", suggestionsNameCell);

            suggestionsListView.DataSource = suggestionsListStore;

            leftPanel.PackStart(suggestionsListView, true, true);
        }

        void GenerateButton_Clicked(object sender, EventArgs e)
        {
            Submit();
        }

        void Submit()
        {
            if (SelectedScaffolder == null || SelectedSuggestion == null)
            {
                return;
            }

            if (!LicensingService.IsPaid)
            {
                WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit($"The File Generation Wizard is a Professional-only MFractor feature. Please upgrade or request a trial.", "File Generation Wizard"));
                return;
            }

            Confirmed?.Invoke(this, new ScaffoldingResultEventArgs(context, Input, SelectedScaffolder, SelectedSuggestion, ScaffolderState.Empty));
        }
    }
}
