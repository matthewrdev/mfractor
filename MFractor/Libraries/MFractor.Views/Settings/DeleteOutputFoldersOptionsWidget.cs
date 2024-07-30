using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Ide.DeleteOutputFolders;
using MFractor.IOC;
using MFractor.Utilities.StringMatchers;
using Xwt;

namespace MFractor.Views.Settings
{
    public class DeleteOutputFoldersOptionsWidget : Xwt.VBox, IOptionsWidget
    {
        public Widget Widget => this;

        public string Title => "Delete Output Folders";

        [Import]
        protected IDeleteOutputFoldersConfigurationService  DeleteOutputFoldersConfigurationService { get; private set; }

        ListStore analysersDataStore;

        readonly DataField<string> analyserNameField = new DataField<string>();
        readonly DataField<bool> deleteBinField = new DataField<bool>();
        readonly DataField<bool> deleteObjField = new DataField<bool>();
        readonly DataField<bool> deletePackageField = new DataField<bool>();
        readonly DataField<bool> deleteVsWorkingFolderField = new DataField<bool>();

        TextCellView analyserNameCell;
        CheckBoxCellView deleteBinCell;
        CheckBoxCellView deleteObjCell;
        CheckBoxCellView deletePackagesCell;
        CheckBoxCellView deleteVsWorkingFolderCell;

        TextEntry searchEntry;

        ListView listView;

        IReadOnlyList<ConfigurationState> configurations;
        Button clearButton;

        public DeleteOutputFoldersOptionsWidget()
        {
            Resolver.ComposeParts(this);

            this.Build();

            BindEvents();

            configurations = DeleteOutputFoldersConfigurationService.Configurations.Select(ca => new ConfigurationState(ca)).ToList();

            ApplyConfigurationsToList();
        }

        void BindEvents()
        {
            UnbindEvents();

            searchEntry.Changed += SearchBar_Changed;
            clearButton.Clicked += ClearButton_Clicked;
        }

        void ClearButton_Clicked(object sender, EventArgs e)
        {
            DeleteOutputFoldersConfigurationService.Clear();

            configurations = DeleteOutputFoldersConfigurationService.Configurations.Select(ca => new ConfigurationState(ca)).ToList();

            ApplyConfigurationsToList();
        }

        void UnbindEvents()
        {
            searchEntry.Changed -= SearchBar_Changed;
            clearButton.Clicked -= ClearButton_Clicked;
        }

        void Build()
        {
            searchEntry = new TextEntry()
            {
                PlaceholderText = "Search for the solution or project",
            };

            PackStart(searchEntry);
            PackStart(new HSeparator());

            BuildListView();

            clearButton = new Button()
            {
                Label = "Clear",
                TooltipText = "Clear all configurations",
            };

            PackStart(clearButton);

        }
        void BuildListView()
        {
            listView = new ListView()
            {
                HeightRequest = 400,
            };

            analysersDataStore = new ListStore(analyserNameField, deleteBinField, deleteObjField, deletePackageField, deleteVsWorkingFolderField);

            deleteBinCell = new CheckBoxCellView(deleteBinField)
            {
                Editable = true,
            };

            deleteObjCell = new CheckBoxCellView(deleteObjField)
            {
                Editable = true,
            };

            deletePackagesCell = new CheckBoxCellView(deletePackageField)
            {
                Editable = true,
            };

            deleteVsWorkingFolderCell = new CheckBoxCellView(deleteVsWorkingFolderField)
            {
                Editable = true,
            };

            analyserNameCell = new TextCellView(analyserNameField)
            {
                Editable = false,
            };

            listView.Columns.Add("Solution/Project", analyserNameCell);
            listView.Columns.Add("bin", deleteBinCell);
            listView.Columns.Add("obj", deleteObjCell);
            listView.Columns.Add("packages", deletePackagesCell);
            listView.Columns.Add(".vs", deleteVsWorkingFolderCell);

            listView.DataSource = analysersDataStore;

            this.PackStart(listView, true, true);
        }

        void SearchBar_Changed(object sender, EventArgs e)
        {
            UnbindEvents();

            try
            {
                ApplyConfigurationsToList();
            }
            finally
            {
                BindEvents();
            }
        }

        readonly LaneStringMatcher filter = new LaneStringMatcher();

        void ApplyConfigurationsToList()
        {
            var sorted = configurations.OrderBy(p => p.Name).ToList();

            if (!string.IsNullOrEmpty(searchEntry.Text))
            {
                filter.SetFilter(searchEntry.Text);
                sorted = filter.Match(sorted, item => item.Name).ToList();
            }

            analysersDataStore.Clear();

            foreach (var analyser in sorted)
            {
                var row = analysersDataStore.AddRow();
                analysersDataStore.SetValue(row, analyserNameField, analyser.Name);
                analysersDataStore.SetValue(row, deleteBinField, analyser.DeleteBin);
                analysersDataStore.SetValue(row, deleteObjField, analyser.DeleteObj);
                analysersDataStore.SetValue(row, deletePackageField, analyser.DeletePackages);
                analysersDataStore.SetValue(row, deleteVsWorkingFolderField, analyser.DeleteVisualStudioWorkingFolder);
            }
        }

        public void ApplyChanges()
        {
            DeleteOutputFoldersConfigurationService.Clear();

            var sorted = configurations.OrderBy(p => p.Name).ToList();

            for (var row = 0; row < analysersDataStore.RowCount; ++row)
            {
                var configuration = sorted[row];

                if (configuration != null)
                {
                    configuration.DeleteBin = analysersDataStore.GetValue(row, deleteBinField);
                    configuration.DeleteObj = analysersDataStore.GetValue(row, deleteObjField);
                    configuration.DeletePackages = analysersDataStore.GetValue(row, deletePackageField);
                    configuration.DeleteVisualStudioWorkingFolder = analysersDataStore.GetValue(row, deleteVsWorkingFolderField);

                    DeleteOutputFoldersConfigurationService.SetOptions(configuration.Name, configuration.Identifier, configuration);
                }
            }
        }

        class ConfigurationState : IDeleteOutputFoldersOptions
        {
            public ConfigurationState(IDeleteOutputFoldersConfiguration configuration)
            {
                Name = configuration.Name;
                Identifier = configuration.Identifier;
                DeleteBin = configuration.Options.DeleteObj;
                DeleteObj = configuration.Options.DeleteObj;
                DeletePackages = configuration.Options.DeletePackages;
                DeleteVisualStudioWorkingFolder = configuration.Options.DeleteVisualStudioWorkingFolder;
            }

            public string Name { get; }

            public string Identifier { get; }
            public bool DeleteBin { get; set; }
            public bool DeleteObj { get; set; }
            public bool DeletePackages { get; set; }
            public bool DeleteVisualStudioWorkingFolder { get; set; }
        }
    }
}
