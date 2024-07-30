using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Code.Analysis;
using MFractor.Documentation;
using MFractor.IOC;
using MFractor.Utilities.StringMatchers;
using Xwt;

namespace MFractor.Views.Settings
{
    public class CodeAnalysisWidget : Xwt.VBox, IOptionsWidget
    {
        public Widget Widget => this;

        public string Title => "Code Analysis";

        [Import]
        protected ICodeAnalysisOptions CodeAnalysisOptions { get; private set; }

        [Import]
        protected ICodeAnalyserRepository CodeAnalyserRepository { get; private set; }

        [Import]
        protected IUrlLauncher UrlLauncher { get; private set; }

        [Import]
        protected IDispatcher Dispatcher { get; private set; }

        [Import]
        protected IFeatureDocumentationService FeatureDocumentationService { get; private set; }

        ListStore analysersDataStore;

        readonly DataField<bool> includeAnalyserField = new DataField<bool>();
        readonly DataField<string> analyserNameField = new DataField<string>();
        readonly DataField<string> analyserHelpMarkupField = new DataField<string>();
        readonly DataField<string> analyserHelpUrlField = new DataField<string>();

        CheckBoxCellView includeAnalyserCell;
        TextCellView analyserNameCell;
        TextCellView analyserHelpCell;

        TextEntry searchEntry;

        ListView anaylsersListView;

        Button enableAllButton;
        Button disableAllButton;

        readonly IReadOnlyList<CodeAnalyserState> analysers;

        public CodeAnalysisWidget()
        {
            Resolver.ComposeParts(this);

            this.Build();

            BindEvents();

            analysers = CodeAnalyserRepository.Analysers.Select(ca =>
            {
                var documentation = FeatureDocumentationService.GetFeatureDocumentationForDiagnostic(ca.DiagnosticId);
                return new CodeAnalyserState(ca, CodeAnalysisOptions, documentation);
            }).ToList();

            ApplyAnalysersToList();
        }

        void BindEvents()
        {
            UnbindEvents();

            enableAllButton.Clicked += EnableAllButton_Clicked;
            disableAllButton.Clicked += DisableAllButton_Clicked;
            includeAnalyserCell.Toggled += IncludeAnalyserCell_Toggled;
            searchEntry.Changed += SearchBar_Changed;
        }

        private void DisableAllButton_Clicked(object sender, EventArgs e)
        {
            foreach (var a in analysers)
            {
                a.IsEnabled = false;
            }

            ApplyAnalysersToList();

        }

        private void EnableAllButton_Clicked(object sender, EventArgs e)
        {
            foreach (var a in analysers)
            {
                a.IsEnabled = true;
            }

            ApplyAnalysersToList();
        }

        void IncludeAnalyserCell_Toggled(object sender, WidgetEventArgs e)
        {
            UnbindEvents();

            Task.Run(async () =>
            {
                await Task.Delay(2);

                Dispatcher.InvokeOnMainThread(() =>
                {

                    try
                    {
                        for (var row = 0; row < analysersDataStore.RowCount; ++row)
                        {
                            var key = analysersDataStore.GetValue(row, analyserNameField);
                            var isChecked = analysersDataStore.GetValue(row, includeAnalyserField);
                            var prop = analysers.FirstOrDefault(p => p.Name == key);

                            if (prop != null)
                            {
                                prop.IsEnabled = isChecked;
                            }
                        }

                    }
                    finally
                    {
                        ApplyAnalysersToList();

                        BindEvents();
                    }
                });
            });
        }

        void UnbindEvents()
        {
            enableAllButton.Clicked -= EnableAllButton_Clicked;
            disableAllButton.Clicked -= DisableAllButton_Clicked;
            includeAnalyserCell.Toggled -= IncludeAnalyserCell_Toggled;
            searchEntry.Changed -= SearchBar_Changed;
        }

        void Build()
        {
            searchEntry = new TextEntry()
            {
                PlaceholderText = "Search for the code analyser by name",
            };

            PackStart(searchEntry);
            PackStart(new HSeparator());

            BuildListView();

            enableAllButton = new Button("Enable All");
            disableAllButton = new Button("Disable All");

            var hbox = new HBox();
            hbox.PackStart(enableAllButton, true);
            hbox.PackStart(disableAllButton, true);

            PackStart(hbox);
        }

        void BuildListView()
        {
            anaylsersListView = new ListView()
            {
                HeightRequest = 400,
            };

            analysersDataStore = new ListStore(includeAnalyserField, analyserNameField, analyserHelpMarkupField, analyserHelpUrlField);

            includeAnalyserCell = new CheckBoxCellView(includeAnalyserField)
            {
                Editable = true,
            };

            analyserNameCell = new TextCellView(analyserNameField)
            {
                Editable = false,
            };

            analyserHelpCell = new TextCellView
            {
                Editable = false,
                MarkupField = analyserHelpMarkupField,
                TextField = analyserHelpUrlField,
            };
            analyserHelpCell.ButtonPressed += AnalyserHelpCell_ButtonPressed;

            anaylsersListView.Columns.Add("", includeAnalyserCell);
            anaylsersListView.Columns.Add("Analyser", analyserNameCell);
            anaylsersListView.Columns.Add("Help Link", analyserHelpCell);

            anaylsersListView.DataSource = analysersDataStore;

            this.PackStart(anaylsersListView, true, true);
        }

        void AnalyserHelpCell_ButtonPressed(object sender, ButtonEventArgs e)
        {
            var cell = (TextCellView)sender;
            if (!string.IsNullOrWhiteSpace(cell.Text))
            {
                UrlLauncher.OpenUrl(cell.Text);
            }
        }

        void SearchBar_Changed(object sender, EventArgs e)
        {
            UnbindEvents();

            try
            {
                ApplyAnalysersToList();
            }
            finally
            {
                BindEvents();
            }
        }

        readonly LaneStringMatcher filter = new LaneStringMatcher();

        void ApplyAnalysersToList()
        {
            var sorted = analysers.OrderBy(p => p.Name).ToList();

            if (!string.IsNullOrEmpty(searchEntry.Text))
            {
                filter.SetFilter(searchEntry.Text);
                try
                {
                    sorted = filter.Match(sorted, item => item.Name).ToList();
                }
                catch { }
            }

            analysersDataStore.Clear();

            foreach (var analyser in sorted)
            {
                var row = analysersDataStore.AddRow();
                analysersDataStore.SetValue(row, includeAnalyserField, analyser.IsEnabled);
                analysersDataStore.SetValue(row, analyserNameField, analyser.Name);

                if (!string.IsNullOrWhiteSpace(analyser.HelpUrl))
                {
                    analysersDataStore.SetValue(row, analyserHelpMarkupField, "Help");
                    analysersDataStore.SetValue(row, analyserHelpUrlField, analyser.HelpUrl);
                }
                else
                {
                    analysersDataStore.SetValue(row, analyserHelpMarkupField, string.Empty);
                    analysersDataStore.SetValue(row, analyserHelpUrlField, string.Empty);
                }
            }
        }

        public void ApplyChanges()
        {
        }

        class CodeAnalyserState
        {
            readonly IFeatureDocumentation featureDocumentation;

            public IXmlSyntaxCodeAnalyser Analyser { get; }

            public ICodeAnalysisOptions Options { get; }

            /// <summary>
            /// Gets the identifier string.
            /// </summary>
            internal string IdString => Analyser.Identifier;

            /// <summary>
            /// Gets the display name for this action.
            /// </summary>
            public string Name => Analyser.Name + " (" + Analyser.DiagnosticId + ")";

            /// <summary>
            /// Gets the Help URL for the analyser documentation.
            /// </summary>
            public string HelpUrl => featureDocumentation?.Url;

            /// <summary>
            /// Gets or sets a value indicating whether this code action is enabled by the user.
            /// </summary>
            /// <value><c>true</c> if this code action is enabled; otherwise, <c>false</c>.</value>
            public bool IsEnabled
            {
                get => Options.IsEnabled(Analyser);
                set => Options.ToggleAnalyser(Analyser, value);
            }

            internal CodeAnalyserState(IXmlSyntaxCodeAnalyser analyser,
                                       ICodeAnalysisOptions options,
                                       IFeatureDocumentation featureDocumentation)
            {
                Analyser = analyser;
                Options = options;
                this.featureDocumentation = featureDocumentation;
            }
        }
    }
}
