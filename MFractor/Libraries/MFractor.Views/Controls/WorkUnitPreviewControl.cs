using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code;
using MFractor.IOC;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Xwt;

namespace MFractor.Views.Controls
{
    /// <summary>
    /// A control used to preview the code/files created by an <see cref="IWorkUnit"/>.
    /// </summary>
    public class WorkUnitPreviewControl : VBox
    {
        ComboBox codeFilesSelectionComboBox;
        ITextEditor textEditor;

        [Import]
        ITextEditorFactory TextEditorFactory { get; set; }

        [Import]
        ICSharpSyntaxReducer SyntaxReducer { get; set; }

        Spinner workingIndicator;
        Label workingStatusLabel;
        HBox workingContainer;

        public WorkUnitPreviewControl()
        {
            Resolver.ComposeParts(this);

            Build();
        }

        bool isWorking = false;
        public bool IsWorking
        {
            get => isWorking;
            set
            {
                isWorking = value;

                workingContainer.Visible = isWorking;
            }
        }

        void Build()
        {
            codeFilesSelectionComboBox = new ComboBox();
            this.PackStart(codeFilesSelectionComboBox);

            textEditor = TextEditorFactory.Create();
            this.PackStart(textEditor.Widget, true, true);

            workingContainer = new HBox();
            workingIndicator = new Spinner()
            {
                Animate = true,
            };
            workingStatusLabel = new Label("Please wait while MFractor loads the code preview...");

            workingContainer.PackStart(workingIndicator);
            workingContainer.PackStart(workingStatusLabel, true, true);
            workingContainer.Visible = false;
            this.PackStart(workingContainer);
        }

        IReadOnlyList<IWorkUnit> workUnitsValues;
        public IReadOnlyList<IWorkUnit> WorkUnits
        {
            get => workUnitsValues;
            set
            {
                workUnitsValues = value;
                ApplyWorkUnits(workUnitsValues);
            }
        }

        void BindEvents()
        {
            UnbindEvents();

            codeFilesSelectionComboBox.SelectionChanged += CodeFilesSelectionComboBox_SelectionChanged;
        }

        void CodeFilesSelectionComboBox_SelectionChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        void UnbindEvents()
        {
            codeFilesSelectionComboBox.SelectionChanged -= CodeFilesSelectionComboBox_SelectionChanged;
        }

        void ProjectPicker_SelectionChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        public void UpdatePreview()
        {
            ApplyWorkUnits(this.WorkUnits);
        }

        void ApplyWorkUnits(IReadOnlyList<IWorkUnit> workUnits)
        {
            UnbindEvents();

            try
            {
                workUnits = workUnits ?? new List<IWorkUnit>();

                var files = workUnits.OfType<CreateProjectFileWorkUnit>();

                var lastSelection = codeFilesSelectionComboBox.SelectedText;

                codeFilesSelectionComboBox.Items.Clear();

                var selectionIndex = 0;
                if (files.Any())
                {
                    var selections = new List<string>();
                    foreach (var f in files)
                    {
                        selections.Add(f.TargetProject.Name + " - " + f.FilePath);
                        codeFilesSelectionComboBox.Items.Add(f, f.TargetProject.Name + " - " + f.FilePath);
                    }

                    if (selections.Contains(lastSelection))
                    {
                        selectionIndex = selections.IndexOf(lastSelection);
                    }

                    codeFilesSelectionComboBox.SelectedIndex = selectionIndex;

                    var selection = codeFilesSelectionComboBox.Items[selectionIndex] as CreateProjectFileWorkUnit;

                    var extension = Path.GetExtension(selection.FilePath);

                    var mimeType = "";
                    var content = selection.FileContent;
                    if (extension == ".xaml")
                    {
                        extension = "application/xaml";
                    }
                    else if (extension == ".cs")
                    {
                        mimeType = "text/x-csharp";
                        content = SyntaxReducer.Reduce(content, selection.TargetProject);
                    }

                    textEditor.MimeType = mimeType;
                    textEditor.Text = content;
                }
                else
                {
                    textEditor.Text = string.Empty;
                }
            }
            finally
            {
                BindEvents();
            }
        }
    }
}
