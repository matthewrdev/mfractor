using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code;
using MFractor.Configuration;
using MFractor.Maui;
using MFractor.Maui.CodeGeneration.ValueConversion;
using MFractor.Maui.Configuration;
using MFractor.IOC;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using Xwt;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Views.ValueConverterWizard
{
    class ValueConverterWizardControl : HBox
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        VBox leftPanel;
        VBox rightPanel;

        ComboBox codeFilesSelectionComboBox;
        ITextEditor textEditor;

        IXamlPlatform platform;
        ProjectIdentifier project;
        Label nameLabel;
        TextEntry nameEntry;

        CheckBox inferTypesCheckbox;

        Label folderPathLabel;
        TextEntry folderPathEntry;

        Label parameterTypeLabel;
        TextEntry parameterTypeEntry;

        Label inputTypeLabel;
        TextEntry inputTypeEntry;

        Label outputTypeLabel;
        TextEntry outputTypeEntry;

        CheckBox addToFileCheckBox;
        ComboBox fileCombo;

        [Import]
        IValueConverterGenerator ValueConverterGenerator { get; set; }

        [Import]
        IValueConversionSettings ValueConversionSettings { get; set; }

        [Import]
        IValueConverterTypeInfermentConfiguration TypeInfermentConfiguration { get; set; }

        [Import]
        IValueConverterTypeInferenceService ValueConverterTypeInferenceService { get; set; }

        [Import]
        ITextEditorFactory TextEditorFactory { get; set; }

        [Import]
        ICSharpSyntaxReducer SyntaxReducer { get; set; }

        internal void Apply(string name, string inputType, string outputType)
        {
            try
            {
                UnbindEvents();

                nameEntry.Text = name;
                inputTypeEntry.Text = inputType;
                outputTypeEntry.Text = outputType;

                ApplyCode();
            }
            finally
            {
                BindEvents();
            }
        }

        internal void SetXamlEntryTargetFiles(bool generateXamlEntry, IReadOnlyList<IProjectFile> projectFiles)
        {

            try
            {
                UnbindEvents();

                addToFileCheckBox.Active = generateXamlEntry;
                fileCombo.Sensitive = generateXamlEntry;

                fileCombo.Items.Clear();

                if (projectFiles != null && projectFiles.Any())
                {
                    foreach (var file in projectFiles)
                    {
                        fileCombo.Items.Add(file, file.VirtualPath);
                    }

                    fileCombo.SelectedItem = projectFiles.FirstOrDefault();
                }

                ApplyCode();
            }
            finally
            {
                BindEvents();
            }
        }

        public ValueConverterWizardControl(IXamlPlatform platform)
        {
            Resolver.ComposeParts(this);

            ValueConverterGenerator.ApplyConfiguration(ConfigurationId.Empty);
            ValueConversionSettings.ApplyConfiguration(ConfigurationId.Empty);
            TypeInfermentConfiguration.ApplyConfiguration(ConfigurationId.Empty);

            Build();

            BindEvents();

            this.platform = platform;
        }

        void BindEvents()
        {
            UnbindEvents();

            nameEntry.Changed += NameEntry_Changed;
            folderPathEntry.Changed += InputChanged;
            inputTypeEntry.Changed += InputChanged;
            outputTypeEntry.Changed += InputChanged;
            parameterTypeEntry.Changed += InputChanged;
            inferTypesCheckbox.Clicked += InputChanged;
            codeFilesSelectionComboBox.SelectionChanged += CodeFilesSelectionComboBox_SelectionChanged;
            addToFileCheckBox.Clicked += AddToFileCheckBox_Clicked;
        }

        void UnbindEvents()
        {
            nameEntry.Changed -= NameEntry_Changed;
            folderPathEntry.Changed -= InputChanged;
            inputTypeEntry.Changed -= InputChanged;
            outputTypeEntry.Changed -= InputChanged;
            parameterTypeEntry.Changed -= InputChanged;
            inferTypesCheckbox.Clicked -= InputChanged;
            codeFilesSelectionComboBox.SelectionChanged -= CodeFilesSelectionComboBox_SelectionChanged;
            addToFileCheckBox.Clicked -= AddToFileCheckBox_Clicked;
        }

        void CodeFilesSelectionComboBox_SelectionChanged(object sender, EventArgs e)
        {
            ApplyCode();
        }

        void AddToFileCheckBox_Clicked(object sender, EventArgs e)
        {
            UnbindEvents();

            try
            {
                fileCombo.Sensitive = addToFileCheckBox.Active;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
            finally
            {
                BindEvents();
            }
        }

        void ApplyCode()
        {
            UnbindEvents();

            try
            {
                var options = this.Options;

                var files = ValueConverterGenerator.Generate(options).OfType<CreateProjectFileWorkUnit>();

                var lastSelectionPath = (codeFilesSelectionComboBox.SelectedItem as CreateProjectFileWorkUnit)?.FilePath;

                codeFilesSelectionComboBox.Items.Clear();

                if (files.Any())
                {
                    foreach (var f in files)
                    {
                        codeFilesSelectionComboBox.Items.Add(f, f.FilePath);
                    }

                    var selection = codeFilesSelectionComboBox.Items.OfType<CreateProjectFileWorkUnit>().FirstOrDefault(cf => cf.FilePath == lastSelectionPath) ?? files.FirstOrDefault();

                    if (selection != null)
                    {
                        codeFilesSelectionComboBox.SelectedItem = selection;
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


        void NameEntry_Changed(object sender, EventArgs e)
        {
            UnbindEvents();
            try
            {
                if (inferTypesCheckbox.Active)
                {
                    var inference = ValueConverterTypeInferenceService.InferTypes(nameEntry.Text, platform, TypeInfermentConfiguration);

                    if (inference.InferenceSuccess)
                    {
                        inputTypeEntry.Text = inference.InputType;
                        outputTypeEntry.Text = inference.OutputType;
                    }
                }

                ApplyCode();
            }
            finally
            {
                BindEvents();
            }
        }

        void InputChanged(object sender, EventArgs e)
        {
            UnbindEvents();
            try
            {
                ApplyCode();
            }
            finally
            {
                BindEvents();
            }
        }

        void Build()
        {
            BuildLeftPanel();

            BuildRightPanel();
        }

        void BuildRightPanel()
        {
            rightPanel = new VBox();

            codeFilesSelectionComboBox = new ComboBox();
            rightPanel.PackStart(codeFilesSelectionComboBox);

            textEditor = TextEditorFactory.Create();
            textEditor.IsReadOnly = true;
            textEditor.MimeType = "text/x-csharp";
            rightPanel.PackStart(textEditor.Widget, true, true);

            PackStart(new HSeparator());

            PackStart(rightPanel, true, true);
        }

        void BuildLeftPanel()
        {
            leftPanel = new VBox()
            {
                MinWidth = 200,
            };
            BuildNameFolderInputs();

            leftPanel.PackStart(new VSeparator());

            BuildConverterTypeInputs();

            BuildAddToFile();

            PackStart(leftPanel);
        }

        void BuildAddToFile()
        {
            leftPanel.PackStart(new HSeparator());

            addToFileCheckBox = new CheckBox()
            {
                Label = "Create XAML Entry",
                TooltipText = "If you would like the new value converter to be added into the current XAML file or App.xaml.",
            };

            leftPanel.PackStart(addToFileCheckBox);

            leftPanel.PackStart(new Label()
            {
                Text = "Add XAML Entry To:"
            });

            fileCombo = new ComboBox();
            leftPanel.PackStart(fileCombo);
        }

        void BuildNameFolderInputs()
        {
            nameLabel = new Label("Name:")
            {
                TooltipText = "What is the name of the new value converter?"
            };
            nameEntry = new TextEntry()
            {
                PlaceholderText = "Value converter name...",
                TooltipText = "What is the name of the new value converter?"
            };

            leftPanel.PackStart(nameLabel);
            leftPanel.PackStart(nameEntry);

            inferTypesCheckbox = new CheckBox("Infer Input/Output Types")
            {
                TooltipText = "When naming your new value converter, MFractor can inspect the name and infer what the input and output types are for your value converter.",
                Active = true,
            };

            leftPanel.PackStart(inferTypesCheckbox);

            folderPathLabel = new Label("Folder:")
            {
                TooltipText = "Where should the new value converter be placed?"
            };

            folderPathEntry = new TextEntry()
            {
                PlaceholderText = "Value converter name...",
                TooltipText = "Where should the new value converter be placed?"
            };

            leftPanel.PackStart(folderPathLabel);
            leftPanel.PackStart(folderPathEntry);
        }

        void BuildConverterTypeInputs()
        {
            inputTypeLabel = new Label("Input Type:")
            {
                TooltipText = "When generating the new value converter, what should it's output type be?"
            };

            inputTypeEntry = new TextEntry()
            {
                PlaceholderText = "Value converter input type...",
                TooltipText = "When generating the new value converter, what should it's input type be?"
            };

            leftPanel.PackStart(inputTypeLabel);
            leftPanel.PackStart(inputTypeEntry);

            outputTypeLabel = new Label("Output Type:")
            {
                TooltipText = "When generating the new value converter, what should it's output type be?"
            };

            outputTypeEntry = new TextEntry()
            {
                PlaceholderText = "Value converter output type...",
                TooltipText = "When generating the new value converter, what should it's output type be?"
            };

            leftPanel.PackStart(outputTypeLabel);
            leftPanel.PackStart(outputTypeEntry);

            parameterTypeLabel = new Label("Parameter Type:")
            {
                TooltipText = "When generating the new value converter, what should it's parameter type be? When empty, no parameter type is generated."
            };

            parameterTypeEntry = new TextEntry()
            {
                PlaceholderText = "Value converter paramater type...",
                TooltipText = "When generating the new value converter, what should it's parameter type be? When empty, no parameter type is generated."
            };

            leftPanel.PackStart(parameterTypeLabel);
            leftPanel.PackStart(parameterTypeEntry);
        }

        public ProjectIdentifier Project
        {
            get => project;
            set
            {
                project = value;

                ApplyConfiguration();
                ApplyCode();
            }
        }

        public ValueConverterGenerationOptions Options
        {
            get
            {
                return new ValueConverterGenerationOptions()
                {
                    Name = nameEntry.Text,
                    Project = project,
                    Platform = platform,
                    Namespace = ValueConversionSettings.CreateConvertersClrNamespace(project, folderPathEntry.Text),
                    FolderPath = folderPathEntry.Text,
                    InputType = inputTypeEntry.Text,
                    OutputType = outputTypeEntry.Text,
                    ParameterType = parameterTypeEntry.Text,
                    XamlEntryProjectFile = addToFileCheckBox.Active ? fileCombo.SelectedItem as IProjectFile : null
                };
            }
        }

        public bool IsValid => !string.IsNullOrEmpty(nameEntry.Text);

        void ApplyConfiguration()
        {
            try
            {
                var configId = ConfigurationId.Create(project);

                ValueConversionSettings.ApplyConfiguration(configId);
                ValueConverterGenerator.ApplyConfiguration(configId);
                TypeInfermentConfiguration.ApplyConfiguration(configId);

                folderPathEntry.Text = ValueConversionSettings.Folder;
                ValueConverterGenerator.CreateMissingValueConversionAttribute = false;
            }
            finally
            {
                BindEvents();
            }
        }
    }
}
